/*
 * Sensor link repository implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using MongoDB.Bson;
using NpgsqlTypes;

using SensateIoT.Platform.Network.Data.Models;
using SensateIoT.Platform.Network.DataAccess.Abstract;
using SensateIoT.Platform.Network.DataAccess.Contexts;
using SensateIoT.Platform.Network.DataAccess.Extensions;

namespace SensateIoT.Platform.Network.DataAccess.Repositories
{
	public class SensorLinkRepository : ISensorLinkRepository
	{
		private readonly NetworkContext m_ctx;

		private const string SelectLinkBySensorID = "networkapi_selectsensorlinkbysensorid";
		private const string SelectLinkByUserID = "networkapi_selectsensorlinkbyuserid";
		private const string CreateSensorLink = "networkapi_createsensorlink";
		private const string DeleteSensorLink = "networkapi_deletesensorlink";
		private const string DeleteSensorLinkBySensorID = "networkapi_deletesensorlinkbysensorid";

		public SensorLinkRepository(NetworkContext context)
		{
			this.m_ctx = context;
		}

		public async Task CreateAsync(SensorLink link, CancellationToken token = default)
		{
			using var builder = StoredProcedureBuilder.Create(this.m_ctx.Database.GetDbConnection());

			builder.WithFunction(CreateSensorLink);
			builder.WithParameter("sensorid", link.SensorId, NpgsqlDbType.Varchar);
			builder.WithParameter("userid", Guid.Parse(link.UserId), NpgsqlDbType.Uuid);

			var reader = await builder.ExecuteAsync(token).ConfigureAwait(false);
			await reader.DisposeAsync().ConfigureAwait(false);
		}

		public async Task<bool> DeleteAsync(SensorLink link, CancellationToken token = default)
		{
			using var builder = StoredProcedureBuilder.Create(this.m_ctx.Database.GetDbConnection());

			builder.WithFunction(DeleteSensorLink);
			builder.WithParameter("sensorid", link.SensorId, NpgsqlDbType.Varchar);
			builder.WithParameter("userid", Guid.Parse(link.UserId), NpgsqlDbType.Uuid);
			await using var reader = await builder.ExecuteAsync(token).ConfigureAwait(false);

			return reader.HasRows;
		}


		public async Task<IEnumerable<SensorLink>> GetAsync(string sensorId, CancellationToken ct = default)
		{
			using var builder = StoredProcedureBuilder.Create(this.m_ctx.Database.GetDbConnection());

			builder.WithFunction(SelectLinkBySensorID);
			builder.WithParameter("sensorid", sensorId, NpgsqlDbType.Varchar);
			await using var reader = await builder.ExecuteAsync(ct).ConfigureAwait(false);

			var list = new List<SensorLink>();

			while(await reader.ReadAsync(ct).ConfigureAwait(false)) {
				var link = new SensorLink {
					SensorId = reader.GetString(0),
					UserId = reader.GetGuid(1).ToString()
				};

				list.Add(link);
			}

			return list;
		}

		public async Task<IEnumerable<SensorLink>> GetByUserAsync(Guid userId, CancellationToken token = default)
		{
			using var builder = StoredProcedureBuilder.Create(this.m_ctx.Database.GetDbConnection());

			builder.WithFunction(SelectLinkByUserID);
			builder.WithParameter("userid", userId, NpgsqlDbType.Uuid);
			await using var reader = await builder.ExecuteAsync(token).ConfigureAwait(false);

			var list = new List<SensorLink>();

			while(await reader.ReadAsync(token).ConfigureAwait(false)) {
				var link = new SensorLink {
					SensorId = reader.GetString(0),
					UserId = reader.GetGuid(1).ToString()
				};

				list.Add(link);
			}

			return list;
		}

		public async Task<bool> DeleteBySensorAsync(ObjectId sensor, CancellationToken ct = default)
		{
			using var builder = StoredProcedureBuilder.Create(this.m_ctx.Database.GetDbConnection());

			builder.WithFunction(DeleteSensorLinkBySensorID);
			builder.WithParameter("sensorid", sensor.ToString(), NpgsqlDbType.Varchar);
			await using var reader = await builder.ExecuteAsync(ct).ConfigureAwait(false);

			return reader.HasRows;
		}
	}
}
