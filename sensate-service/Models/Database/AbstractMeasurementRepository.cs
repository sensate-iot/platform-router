/*
 * Abstract measurement repository
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MongoDB.Driver;
using MongoDB.Bson;

namespace SensateService.Models.Database
{
	internal class RawMeasurement
	{
		public decimal Data {get;set;}
		public double Longitude {get;set;}
		public double Latitude {get;set;}
		public DateTime CreatedAt {get;set;}
		public string CreatedBySecret {get;set;}
	}

	public abstract class AbstractMeasurementRepository : AbstractDocumentRepository<Measurement>
	{
		public event OnMeasurementReceived MeasurementReceived;

		private readonly IMongoCollection<Measurement> _measurements;
		private readonly Random _random;
		private ILogger<AbstractMeasurementRepository> _logger;

		public AbstractMeasurementRepository(SensateContext context,
			ILogger<AbstractMeasurementRepository> logger) : base(context)
		{
			this._measurements = context.Measurements;
			this._random = new Random();
			this._logger = logger;
		}

		protected long GenerateId(DateTime ts)
		{
			long id;
			int x, y;
			long rand;

			id = ts.Ticks;
			this.GenerateId(out x, out y);
			rand = x << 32;;
			rand |= (long)y;

			/*
			 * Randomize the tick value
			 */
			id += y;
			id *= rand;
			id += x;

			id = (long) ((ulong)id) & 0x7FFFFFFFFFFFFFFFL;
			return id;
		}

		private void GenerateId(out int x, out int y)
		{
			x = this._random.Next();
			y = this._random.Next();
		}

		public override bool Update(Measurement obj)
		{
			var update = Builders<Measurement>.Update
				.Set(x => x.Data, obj.Data)
				.Set(x => x.Latitude, obj.Latitude)
				.Set(x => x.Longitude, obj.Longitude);

			try {
				var result = this._measurements.FindOneAndUpdate(
					x => x.Id == obj.Id,
					update
				);

				return result != null;
			} catch(Exception ex) {
				this._logger.LogInformation($"Failed to update measurement: {ex.Message}");
				return false;
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

		public sealed override bool Create(Measurement m)
		{
			throw new NotImplementedException();
		}

		public override bool Delete(long id)
		{
			var result = this._measurements.DeleteOne(
				x => x.Id == id ||
				x.InternalId == ToInternalId(id)
			);
			return result.DeletedCount > 0;
		}

		public override Measurement GetById(long id)
		{
			var result = this._measurements.Find(x => x.Id == id);
			return result.FirstOrDefault();
		}

		public override bool Replace(Measurement obj1, Measurement obj2)
		{
			UpdateResult result;
			var update = Builders<Measurement>.Update
				.Set(x => x.Data, obj2.Data)
				.Set(x => x.Latitude, obj2.Latitude)
				.Set(x => x.Longitude, obj2.Longitude);

			try {
				result = this._measurements.UpdateOne(
					x => x.Id == obj2.Id || x.InternalId == obj2.InternalId,
					update
				);
			} catch (Exception ex) {
				this._logger.LogInformation($"Failed to update measurement: {ex.Message}");
				return false;
			}

			return result.ModifiedCount > 0;
		}

		public async void ReceiveMeasurement(Sensor sender, string measurement)
		{
			MeasurementReceivedEventArgs args;
			Measurement m;

			m = await this.StoreMeasurement(sender, measurement);
			if(m != null) {
				args = new MeasurementReceivedEventArgs() {
					Measurement = m
				};

				if(this.MeasurementReceived != null)
					await this.MeasurementReceived(sender, args);
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
			string key;

			key = $"{sensor.Secret}::after::{pit.ToString()}";
			return this.TryGetMeasurements(key, x =>
				x.CreatedBy == sensor.InternalId && x.CreatedAt.CompareTo(pit) >= 0
			);
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
			string key;

			key = $"{sensor.Secret}::after::{pit.ToString()}";
			return await this.TryGetMeasurementsAsync(key, x =>
				x.CreatedBy == sensor.InternalId && x.CreatedAt.CompareTo(pit) >= 0
			);
		}

		private async Task<Measurement> StoreMeasurement(Sensor sensor, string json)
		{
			Measurement m;
			RawMeasurement raw;
			DateTime now;

			if(json == null || sensor == null)
				return null;

			try {
				raw = JsonConvert.DeserializeObject<RawMeasurement>(json);

				if(raw == null || raw.CreatedBySecret != sensor.Secret)
					return null;
			} catch(JsonSerializationException ex) {
				this._logger.LogInformation($"Bad measurement received: ${ex.Message}");
				return null;
			}

			now = DateTime.Now;
			if(raw.CreatedAt == null || raw.CreatedAt.CompareTo(DateTime.MinValue) <= 0)
				raw.CreatedAt = now;

			m = new Measurement {
				Data = raw.Data,
				CreatedAt = raw.CreatedAt,
				Longitude = raw.Longitude,
				Latitude = raw.Latitude,
				CreatedBy = sensor.InternalId,
				Id = this.GenerateId(now),
				InternalId = ObjectId.GenerateNewId(now)
			};

			await this._measurements.InsertOneAsync(m);
			await this.CommitAsync(m);

			return m;
		}
	}
}
