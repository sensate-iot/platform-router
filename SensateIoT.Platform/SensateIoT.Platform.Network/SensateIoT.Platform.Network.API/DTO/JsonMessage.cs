/*
 * JSON message.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;

namespace SensateIoT.Platform.Network.API.DTO
{
	public class JsonMessage : Tuple<Message, string>
	{
		public JsonMessage(Message m, string t) : base(m, t)
		{
		}
	}
}
