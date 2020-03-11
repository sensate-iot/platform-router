/*
 * Internal measurement model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

namespace SensateService.TriggerHandler.Models
{
	public class InternalMeasurement<T>
	{
		public T Measurements { get; set; }
		public string CreatedBy { get; set; }
	}
}

