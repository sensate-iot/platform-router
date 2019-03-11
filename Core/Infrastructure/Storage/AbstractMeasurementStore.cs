/*
 * Abstract measurement store definition. This class implements
 * abstract storage features, such as the validation of received
 * measurements.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SensateService.Enums;
using SensateService.Exceptions;
using SensateService.Helpers;
using SensateService.Models;
using SensateService.Models.Json.In;

namespace SensateService.Infrastructure.Storage
{
	public abstract class AbstractMeasurementStore : IMeasurementStore
	{
		protected IServiceProvider Provider { get; }
		protected ILogger Logger { get; }

		private const int MaxDatapointLength = 25;

		protected AbstractMeasurementStore(IServiceProvider provider, ILogger logger)
		{
			this.Provider = provider;
			this.Logger = logger;
		}

		protected Measurement ProcessRawMeasurement(Sensor sensor, RawMeasurement obj)
		{
			Measurement measurement;
			IList<DataPoint> datapoints;

			if(Math.Abs(obj.Latitude) < double.Epsilon && Math.Abs(obj.Longitude) < double.Epsilon)
				throw new InvalidRequestException(ErrorCode.InvalidDataError.ToInt(), "Invalid measurement location!");

			if(!obj.IsCreatedBy(sensor))
				return null;

			if(obj.TryParseData(out var data)) {
				datapoints = data as IList<DataPoint>;

				if(datapoints == null)
					return null;

				if(datapoints.Count <= 0 || datapoints.Count >= MaxDatapointLength)
					return null;
			} else {
				return null;
			}

			measurement = new Measurement {
				CreatedBy = sensor.InternalId,
				Data = datapoints,
				Longitude = obj.Longitude,
				Latitude = obj.Latitude,
				CreatedAt = obj.CreatedAt ?? DateTime.Now.ToUniversalTime()
			};

			return measurement;
		}

		protected bool CanInsert(IEnumerable<string> roles)
		{
			return roles.Contains(SensateRole.Banned);
		}

		protected bool CanInsert(SensateUser user)
		{
			return user.UserRoles.Any(role => role.Role.Name != SensateRole.Banned);
		}

		public abstract Task StoreAsync(RawMeasurement obj, RequestMethod method);
	}
}
