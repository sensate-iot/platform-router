/*
 * A trigger/action combination DTO.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;

using MongoDB.Bson;

namespace SensateIoT.Platform.Network.Data.DTO
{
	public class TriggerAction
	{
		public long TriggerID { get; set; }
		public long ActionID { get; set; }
		public ObjectId SensorID { get; set; }
		public string KeyValue { get; set; }
		public decimal? LowerEdge { get; set; }
		public decimal? UpperEdge { get; set; }
		public string FormalLanguage { get; set; }
		public TriggerType Type { get; set; }
		public TriggerChannel Channel { get; set; }
		public string Target { get; set; }
		public string Message { get; set; }
	}
}
