﻿/*
 * Trigger action model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.ComponentModel.DataAnnotations;

namespace SensateService.Common.Data.Models
{
	public enum TriggerActionChannel
	{
		Email,
		SMS,
		MQTT,
		HttpPost,
		HttpGet,
		ControlMessage
	}

	public class TriggerAction
	{
		[Required]
		public long TriggerId { get; set; }
		[Required]
		public TriggerActionChannel Channel { get; set; }
		public string Target { get; set; }
		[Required, StringLength(255)]
		public string Message { get; set; }
	}
}