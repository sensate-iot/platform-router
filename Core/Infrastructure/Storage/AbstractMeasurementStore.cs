/*
 * Abstract measurement store definition. This class implements
 * abstract storage features, such as the authorization and
 * validation of received measurements.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using MongoDB.Driver.GeoJsonObjectModel;

using SensateService.Crypto;
using SensateService.Enums;
using SensateService.Models;
using SensateService.Models.Json.In;

namespace SensateService.Infrastructure.Storage
{
	using ValidationData = Tuple<IDictionary<string, Sensor>, IDictionary<string, SensateUser>>;
	using ParsedMeasurementEntry = Tuple<RequestMethod, RawMeasurement, string>;

	public abstract class AbstractMeasurementStore : IMeasurementStore
	{
		protected ILogger Logger { get; }
		private readonly IHashAlgorithm m_algo;

		private const int MaxDatapointLength = 25;
		private const int SecretSubStringOffset = 3;
		private const int SecretSubStringStart = 1;

		protected AbstractMeasurementStore(IHashAlgorithm algo, ILogger logger)
		{
			this.Logger = logger;
			this.m_algo = algo;
		}

		protected Measurement ProcessRawMeasurement(RawMeasurement obj)
		{
			Measurement measurement;
			IDictionary<string, DataPoint> datapoints;
			GeoJsonPoint<GeoJson2DGeographicCoordinates> point;

			if(obj.TryParseData(out var data)) {
				datapoints = data;

				if(datapoints == null) {
					return null;
				}

				if(datapoints.Count <= 0 || datapoints.Count >= MaxDatapointLength) {
					return null;
				}
			} else {
				return null;
			}

			if(obj.Latitude.HasValue && obj.Longitude.HasValue) {
				var coords = new GeoJson2DGeographicCoordinates(obj.Longitude.Value, obj.Latitude.Value);
				point = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(coords);
			} else {
				point = null;
			}

			measurement = new Measurement {
				Data = datapoints,
				Timestamp = obj.CreatedAt ?? DateTime.UtcNow,
				Location = point,
				PlatformTime = DateTime.UtcNow
			};

			return measurement;
		}
		private static byte[] HexToByteArray(string hex)
		{
			var num = hex.Length;
			var bytes = new byte[num / 2];

			for(var i = 0; i < num; i += 2) {
				bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
			}

			return bytes;
		}

		private static bool CompareHashes(ReadOnlySpan<byte> h1, ReadOnlySpan<byte> h2)
		{
			return h1.SequenceEqual(h2);
		}

		protected Measurement AuthorizeMeasurement(Sensor sensor, SensateUser user, ParsedMeasurementEntry raw)
		{
			RawMeasurement measurement;
			Regex search;
			Regex match;

			if(!this.CanInsert(user)) {
				return null;
			}

			measurement = raw.Item2;
			match = this.m_algo.GetMatchRegex();
			search = this.m_algo.GetSearchRegex();

			if(match.IsMatch(measurement.CreatedBySecret)) {
				var length = measurement.CreatedBySecret.Length - SecretSubStringOffset;
				var json = search.Replace(raw.Item3, sensor.Secret, 1);
				var binary = Encoding.ASCII.GetBytes(json);
				var computed = this.m_algo.ComputeHash(binary);
				var hash = HexToByteArray(measurement.CreatedBySecret.Substring(SecretSubStringStart, length));

				if(!CompareHashes(computed, hash)) {
					return null;
				}
			} else {
				if(!(this.InsertAllowed(user, measurement.CreatedBySecret) && measurement.IsCreatedBy(sensor))) {
					return null;
				}
			}

			return this.ProcessRawMeasurement(measurement);
		}

		protected IList<ProcessedMeasurement> AuthorizeMeasurements(ValidationData data, IList<ParsedMeasurementEntry> measurements)
		{
			ProcessedMeasurement[] processed;
			bool rv;

			processed = new ProcessedMeasurement[measurements.Count];

			try {
				var result = Parallel.For(0, measurements.Count, (index, state) => {
					var measurement = measurements[index].Item2;

					if(!data.Item1.TryGetValue(measurement.CreatedById, out var sensor)) {
						measurements[index] = null;
						return;
					}

					if(!data.Item2.TryGetValue(sensor.Owner, out var user)) {
						measurements[index] = null;
						return;
					}

					var tmp = this.AuthorizeMeasurement(sensor, user, measurements[index]);

					if(tmp == null) {
						processed[index] = null;
						return;
					}

					processed[index] = new ProcessedMeasurement(tmp, sensor, measurements[index].Item1);
				});

				rv = result.IsCompleted;
			} catch(Exception ex) {
				this.Logger.LogInformation($"Unable to authorize measurements: {ex.Message}!");
				this.Logger.LogDebug(ex.StackTrace);

				rv = false;
			}

			if(!rv) {
				return new List<ProcessedMeasurement>();
			}

			var aslist = processed.ToList();
			aslist.RemoveAll(x => x == null);

			return aslist;
		}

		protected bool CanInsert(SensateUser user)
		{
			var banned = SensateRole.Banned.ToUpperInvariant();
			return user.UserRoles.All(role => role.Role.NormalizedName != banned);
		}

		protected bool InsertAllowed(SensateUser user, string key)
		{
			var apikey = user.ApiKeys.FirstOrDefault(k => k.ApiKey == key);

			if(apikey == null) {
				return false;
			}

			return !apikey.Revoked && apikey.Type == ApiKeyType.SensorKey;
		}

		public abstract Task StoreAsync(string obj, RequestMethod method);
	}

	public class ProcessedMeasurement
	{
		public Measurement Measurement { get; }
		public Sensor Creator { get; }
		public RequestMethod Method { get; }

		public ProcessedMeasurement(Measurement measurement, Sensor creator, RequestMethod method)
		{
			this.Measurement = measurement;
			this.Creator = creator;
			this.Method = method;
		}
	}
}
