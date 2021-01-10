/*
 * Message with its origin JSON representation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

namespace SensateIoT.API.Common.Data.Dto.Authorization
{
	public class JsonMessage
	{
		public string Json { get; set; }
		public Message Message { get; set; }
	}
}
