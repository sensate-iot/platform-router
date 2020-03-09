/*
 * Internal measurement model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using SensateService.Models;

namespace SensateService.TriggerHandler.Models
{
	public class InternalMeasurement
	{
		public List<Measurement> Measurements { get; set; }
		public string CreatedBy { get; set; }
	}
}

