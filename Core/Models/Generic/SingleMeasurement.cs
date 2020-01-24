/*
 * Measurement model for a single measurement.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using MongoDB.Bson;

namespace SensateService.Models.Generic
{
	public class SingleMeasurement
	{
        public ObjectId Id { get; set; }
		public ObjectId SensorId { get; set; }
        public Measurement Measurement { get; set; }
	}
}