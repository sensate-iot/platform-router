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
	public class InternalBulkMessageQueue
	{
		public IList<Message> Messages { get; set; }
		public string SensorId { get; set; }
	}
}