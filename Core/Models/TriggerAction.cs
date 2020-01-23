/*
 * Trigger action model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.ComponentModel.DataAnnotations;

namespace SensateService.Models
{
	public enum TriggerActionChannel
	{
		Emain,
		SMS,
		MQTT
	}

	public class TriggerAction
	{
		[Required]
		public long TriggerId { get; set; }
		[Required]
		public TriggerActionChannel Channel { get; set; }
	}
}