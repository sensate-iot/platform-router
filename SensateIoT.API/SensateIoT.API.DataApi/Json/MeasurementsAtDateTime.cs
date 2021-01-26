/*
 * Statistics at a timestamp.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;

namespace SensateIoT.API.DataApi.Json
{
	public class MeasurementsAtDateTime
	{
		public DateTime Timestamp { get; set; }
		public long Measurements { get; set; }
	}
}