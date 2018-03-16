/*
 * MongoDB measurement repository implementation.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */


using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

using Microsoft.Extensions.Logging;

using MongoDB.Driver;
using MongoDB.Bson;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using SensateService.Infrastructure.Events;
using SensateService.Models;
using SensateService.Exceptions;
using SensateService.Infrastructure.Repositories;

namespace SensateService.Infrastructure.Document
{
	public class MeasurementRepository : AbstractDocumentRepository<string, Measurement>, IMeasurementRepository
	{
		private readonly IMongoCollection<Measurement> _measurements;
		private readonly Random _random;
		protected readonly ILogger<MeasurementRepository> _logger;

		public const int JsonError = 300;
		public const int IncorrectSecretError = 301;
		public const int InvalidDataError = 302;

		public MeasurementRepository(SensateContext context, ILogger<MeasurementRepository> logger)
			: base(context)
		{
			this._measurements = context.Measurements;
			this._random = new Random();
			this._logger = logger;
		}

		protected ObjectId ToInternalId(string id)
		{
			ObjectId internalId;

			if(!ObjectId.TryParse(id, out internalId))
				internalId = ObjectId.Empty;

			return internalId;
		}

		public override void Commit(Measurement obj)
		{
			return;
		}

		public async override Task CommitAsync(Measurement obj)
		{
			await Task.CompletedTask;
		}

		public override void Update(Measurement obj)
		{
			var update = Builders<Measurement>.Update
				.Set(x => x.Data, obj.Data)
				.Set(x => x.Latitude, obj.Latitude)
				.Set(x => x.Longitude, obj.Longitude);

			try {
				this._measurements.FindOneAndUpdate(
					x => x.InternalId == obj.InternalId,
					update
				);

			} catch(Exception ex) {
				this._logger.LogInformation($"Failed to update measurement: {ex.Message}");
			}
		}

		public virtual async Task<IEnumerable<Measurement>> GetMeasurementsBySensorAsync(Sensor sensor)
		{
			var query = Builders<Measurement>.Filter.Eq("CreatedBy", sensor.InternalId);

			try {
				var result = await this._measurements.FindAsync(query);
				return result.ToEnumerable();
			} catch (Exception ex) {
				this._logger.LogWarning(ex.Message);
				return null;
			}
		}

		public virtual IEnumerable<Measurement> GetMeasurementsBySensor(Sensor sensor)
		{
			var query = Builders<Measurement>.Filter.Eq("CreatedBy", sensor.InternalId);

			try {
				return this._measurements.Find(query).ToEnumerable();
			} catch (Exception ex) {
				this._logger.LogWarning(ex.Message);
				return null;
			}
		}

		public override void Create(Measurement m)
		{
			if(m.CreatedBy == null || m.CreatedBy == ObjectId.Empty)
				return;

			m.CreatedAt = DateTime.Now;
			m.InternalId = this.GenerateId(DateTime.Now);
			this._measurements.InsertOne(m);
			this.Commit(m);
		}

		public async override Task CreateAsync(Measurement obj)
		{
			if(obj.CreatedBy == null || obj.CreatedBy == ObjectId.Empty)
				return;

			obj.CreatedAt = DateTime.Now;
			obj.InternalId = this.GenerateId(DateTime.Now);
			await this._measurements.InsertOneAsync(obj);
			await this.CommitAsync(obj);
		}

		public override void Delete(string id)
		{
			ObjectId oid;

			oid = this.ToInternalId(id);
			if(oid == null)
				return;

			this._measurements.DeleteOne(x =>
				x.InternalId == oid
			);
		}

		public override async Task DeleteAsync(string id)
		{
			ObjectId objectId;

			objectId = this.ToInternalId(id);
			if(objectId == null)
				return;

			await this._measurements.DeleteOneAsync(x => x.InternalId == objectId);
		}

		public override Measurement GetById(string id)
		{
			ObjectId oid = this.ToInternalId(id);
			var find = Builders<Measurement>.Filter.Eq("InternalId", oid);
			var result = this._measurements.Find(find);

			if(result != null)
				return result.FirstOrDefault();

			return null;
		}

		public async Task ReceiveMeasurement(Sensor sender, string measurement)
		{
			MeasurementReceivedEventArgs args;
			Measurement m;

			m = await this.StoreMeasurement(sender, measurement);
			if(m != null) {
				args = new MeasurementReceivedEventArgs() {
					Measurement = m
				};

				await MeasurementEvents.OnMeasurementReceived(sender, args);
			}
		}

		public virtual Measurement TryGetMeasurement(
			string key,
			Expression<Func<Measurement, bool>> expression
		)
		{
			return this._measurements.FindSync(expression).FirstOrDefault();
		}

		public async virtual Task<Measurement> TryGetMeasurementAsync(
			string key, Expression<Func<Measurement, bool>> expression)
		{
			IAsyncCursor<Measurement> result;

			result = await this._measurements.FindAsync(expression);
			return await result.FirstOrDefaultAsync();
		}

		public async virtual Task<IEnumerable<Measurement>> TryGetMeasurementsAsync(
			string key, Expression<Func<Measurement, bool>> expression)
		{
			var result = await this._measurements.FindAsync(expression);
			return result.ToEnumerable();
		}

		public virtual IEnumerable<Measurement> TryGetMeasurements(
			string key, Expression<Func<Measurement, bool>> expression)
		{
			var result = this._measurements.Find(expression);
			return result.ToEnumerable();
		}

		private async Task<Measurement> StoreMeasurement(Sensor sensor, string json)
		{
			Measurement m;
			RawMeasurement raw;
			DateTime now;
			BsonDocument document;

			if(json == null || sensor == null)
				return null;

			try {
				raw = Newtonsoft.Json.JsonConvert.DeserializeObject<RawMeasurement>(json);

				if(raw == null || raw.CreatedBySecret != sensor.Secret) {
					throw new InvalidRequestException(
						MeasurementRepository.IncorrectSecretError,
						"Sensor secret doesn't match sensor ID!"
					);
				}
			} catch(JsonSerializationException ex) {
				this._logger.LogInformation($"Bad measurement received: ${ex.Message}");
				throw new InvalidRequestException(MeasurementRepository.JsonError);
			}

			now = DateTime.Now;
			if(raw.CreatedAt == null || raw.CreatedAt.CompareTo(DateTime.MinValue) <= 0)
				raw.CreatedAt = now;

			m = new Measurement {
				CreatedAt = raw.CreatedAt,
				Longitude = raw.Longitude,
				Latitude = raw.Latitude,
				CreatedBy = sensor.InternalId,
				InternalId = base.GenerateId(now)
			};

			if(BsonDocument.TryParse(raw.Data.ToString(), out document)) {
				m.Data = document;
			} else {
				throw new InvalidRequestException(
					MeasurementRepository.InvalidDataError,
					"Unable to parse data"
				);
			}

			try {
				var opts = new InsertOneOptions {
					BypassDocumentValidation = true
				};

				await this._measurements.InsertOneAsync(m, opts, CancellationToken.None);
				await this.CommitAsync(m);
			} catch(Exception ex) {
				this._logger.LogWarning($"Unable to insert measurement: {ex.Message}");
				throw new DatabaseException(
					$"Unable to store message: {ex.Message}",
					"Measurements", ex
				);
			}

			return m;
		}

		public virtual IEnumerable<Measurement> TryGetBetween(Sensor sensor, DateTime start, DateTime end)
		{
			return this.TryGetMeasurements(null, x =>
				x.CreatedBy == sensor.InternalId &&
				x.CreatedAt.CompareTo(start) >= 0 && x.CreatedAt.CompareTo(end) <= 0
			);
		}

		public virtual async Task<IEnumerable<Measurement>> TryGetBetweenAsync(
			Sensor sensor, DateTime start, DateTime end
		)
		{
			return await this.TryGetMeasurementsAsync(null, x =>
				x.CreatedBy == sensor.InternalId &&
				x.CreatedAt.CompareTo(start) >= 0 && x.CreatedAt.CompareTo(end) <= 0
			);
		}

		public virtual IEnumerable<Measurement> GetBefore(Sensor sensor, DateTime pit)
		{
			string key;

			key = $"{sensor.Secret}::before::{pit.ToString()}";
			return this.TryGetMeasurements(key, x =>
				x.CreatedBy == sensor.InternalId && x.CreatedAt.CompareTo(pit) <= 0
			);
		}

		public virtual IEnumerable<Measurement> GetAfter(Sensor sensor, DateTime pit)
		{
			return this._measurements.Find(x =>
				x.CreatedBy == sensor.InternalId && x.CreatedAt.CompareTo(pit) >= 0
			).ToEnumerable();
		}

		public virtual async Task<IEnumerable<Measurement>> GetBeforeAsync(Sensor sensor, DateTime pit)
		{
			string key;

			key = $"{sensor.Secret}::before::{pit.ToString()}";
			return await this.TryGetMeasurementsAsync(key, x =>
				x.CreatedBy == sensor.InternalId && x.CreatedAt.CompareTo(pit) <= 0
			);
		}

		public virtual async Task<IEnumerable<Measurement>> GetAfterAsync(Sensor sensor, DateTime pit)
		{
			var result = await this._measurements.FindAsync(x =>
				x.CreatedBy == sensor.InternalId && x.CreatedAt.CompareTo(pit) >= 0
			);
			return result.ToEnumerable();
		}

		public virtual Measurement GetMeasurement(string key, Expression<Func<Measurement, bool>> selector)
		{
			return this.TryGetMeasurement(key, selector);
		}

		public virtual async Task<Measurement> GetMeasurementAsync(string key, Expression<Func<Measurement, bool>> selector)
		{
			return await this.TryGetMeasurementAsync(key, selector);
		}
	}

	internal class RawMeasurement
	{
		public JObject Data {get;set;}
		public double Longitude {get;set;}
		public double Latitude {get;set;}
		public DateTime CreatedAt {get;set;}
		public string CreatedBySecret {get;set;}
	}
}
