/*
 * Data repository for sensor information.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using MongoDB.Bson;
using MongoDB.Driver;

using NpgsqlTypes;

using SensateIoT.Platform.Network.Data.Models;
using SensateIoT.Platform.Network.DataAccess.Abstract;
using SensateIoT.Platform.Network.DataAccess.Contexts;
using SensateIoT.Platform.Network.DataAccess.Extensions;

using Sensor = SensateIoT.Platform.Network.Data.Models.Sensor;

namespace SensateIoT.Platform.Network.DataAccess.Repositories
{
	public class SensorRepository : ISensorRepository
	{
		private readonly IMongoCollection<Sensor> m_sensors;
		private readonly NetworkContext m_network;

		private const string NetworkApi_DeleteBySensorIds = "NetworkApi_DeleteBySensorIds";

		public SensorRepository(MongoDBContext ctx, NetworkContext network)
		{
			this.m_sensors = ctx.Sensors;
			this.m_network = network;
		}

		public async Task CreateAsync(Sensor sensor, CancellationToken ct = default)
		{
			var now = DateTime.UtcNow;

			sensor.CreatedAt = now;
			sensor.UpdatedAt = now;
			sensor.InternalId = ObjectId.GenerateNewId();

			await this.m_sensors.InsertOneAsync(sensor, default, ct).ConfigureAwait(false);
		}

		public async Task<IEnumerable<Sensor>> GetAsync(Guid userId, int skip = 0, int limit = 0)
		{
			FilterDefinition<Sensor> filter;
			var builder = Builders<Sensor>.Filter;
			var id = userId.ToString();

			filter = builder.Where(s => s.Owner == id);
			var sensors = this.m_sensors.Aggregate().Match(filter);

			if(sensors == null) {
				return null;
			}

			if(skip > 0) {
				sensors = sensors.Skip(skip);
			}

			if(limit > 0) {
				sensors = sensors.Limit(limit);
			}

			return await sensors.ToListAsync().ConfigureAwait(false);
		}

		public async Task<Sensor> GetAsync(ObjectId id, CancellationToken ct)
		{
			var filter = Builders<Sensor>.Filter.Where(x => x.InternalId == id);
			var result = await this.m_sensors.FindAsync(filter).ConfigureAwait(false);

			if(result == null) {
				return null;
			}

			return await result.FirstOrDefaultAsync(ct).ConfigureAwait(false);
		}

		public async Task<Sensor> GetAsync(string id, CancellationToken ct)
		{
			if(!ObjectId.TryParse(id, out var oid)) {
				throw new FormatException("Invalid SensorID format!");
			}

			return await this.GetAsync(oid, ct);
		}

		public async Task<IEnumerable<Sensor>> GetAsync(IEnumerable<string> ids)
		{
			FilterDefinition<Sensor> filter;
			var builder = Builders<Sensor>.Filter;
			var idlist = new List<ObjectId>();

			foreach(var id in ids) {
				if(!ObjectId.TryParse(id, out var parsedId)) {
					continue;
				}

				idlist.Add(parsedId);
			}

			filter = builder.In(x => x.InternalId, idlist);
			var raw = await this.m_sensors.FindAsync(filter).ConfigureAwait(false);
			return raw.ToList();
		}

		public async Task<IEnumerable<Sensor>> FindByNameAsync(Guid userId, string name, int skip = 0, int limit = 0)
		{
			FilterDefinition<Sensor> filter;
			var builder = Builders<Sensor>.Filter;
			var uid = userId.ToString();

			filter = builder.Where(x => x.Name.Contains(name)) &
					 builder.Eq(x => x.Owner, uid);
			var sensors = this.m_sensors.Aggregate().Match(filter);

			if(sensors == null) {
				return null;
			}

			if(skip > 0) {
				sensors = sensors.Skip(skip);
			}

			if(limit > 0) {
				sensors = sensors.Limit(limit);
			}

			return await sensors.ToListAsync().ConfigureAwait(false);
		}

		public async Task<long> CountAsync(User user)
		{
			FilterDefinition<Sensor> filter;

			if(user == null) {
				return await this.m_sensors.CountDocumentsAsync(new BsonDocument()).ConfigureAwait(false);
			}

			var uid = user.ID.ToString();
			var builder = Builders<Sensor>.Filter;
			filter = builder.Where(s => s.Owner == uid);
			return await this.m_sensors.CountDocumentsAsync(filter).ConfigureAwait(false);
		}

		public async Task<long> CountAsync(User user, string name)
		{
			FilterDefinition<Sensor> filter;
			var builder = Builders<Sensor>.Filter;
			var uid = user.ID.ToString();

			filter = builder.Where(x => x.Owner == uid && x.Name.Contains(name));
			return await this.m_sensors.CountDocumentsAsync(filter).ConfigureAwait(false);
		}

		public async Task DeleteAsync(IEnumerable<ObjectId> sensorIds, CancellationToken ct = default)
		{
			using var builder = StoredProcedureBuilder.Create(this.m_network.Database.GetDbConnection());
			var sensorIdList = sensorIds.ToList();

			var idlist = string.Join(',', sensorIdList.Select(x => x.ToString()));
			builder.WithParameter("idlist", idlist, NpgsqlDbType.Text);
			builder.WithFunction(NetworkApi_DeleteBySensorIds);

			var networkTask = builder.ExecuteAsync(ct).ConfigureAwait(false);

			var filter = Builders<Sensor>.Filter.In(x => x.InternalId, sensorIdList);
			await this.m_sensors.DeleteManyAsync(filter, ct).ConfigureAwait(false);

			var reader = await networkTask;
			await reader.DisposeAsync().ConfigureAwait(false);
		}

		public async Task DeleteAsync(ObjectId sensorId, CancellationToken ct = default)
		{
			await this.m_sensors.DeleteOneAsync(x => x.InternalId == sensorId, ct).ConfigureAwait(false);
		}

		public async Task UpdateAsync(Sensor sensor)
		{
			var update = Builders<Sensor>.Update.Set(x => x.UpdatedAt, DateTime.UtcNow);

			if(sensor.Name != null) {
				update = update.Set(x => x.Name, sensor.Name);
			}

			if(sensor.Description != null) {
				update = update.Set(x => x.Description, sensor.Description);
			}

			update = update.Set(x => x.StorageEnabled, sensor.StorageEnabled);

			await this.m_sensors.FindOneAndUpdateAsync(x => x.InternalId == sensor.InternalId, update).ConfigureAwait(false);
		}

		public async Task UpdateSecretAsync(ObjectId sensorId, string key)
		{
			var update = Builders<Sensor>.Update.Set(x => x.UpdatedAt, DateTime.UtcNow)
				.Set(x => x.Secret, key);
			await this.m_sensors.FindOneAndUpdateAsync(x => x.InternalId == sensorId, update)
				.ConfigureAwait(false);
		}
	}
}
