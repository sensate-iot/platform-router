/*
 * Measurement geo query result model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using MongoDB.Bson;

namespace SensateService.Models.Generic
{
	public class MeasurementsGeoQueryResult
	{
        public ObjectId Id { get; set; }
		public double Distance { get; set; }
        public IEnumerable<Measurement> Measurements { get; set; }

	}
}