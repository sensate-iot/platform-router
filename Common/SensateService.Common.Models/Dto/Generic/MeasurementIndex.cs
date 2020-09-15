/*
 * Lookup model for measurements.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace SensateService.Common.Data.Dto.Generic
{
	public class MeasurementIndex
	{
		[Required]
		public ObjectId MeasurementBucketId { get; set; }
		[Required]
		public int Index { get; set; }
	}
}