/*
 * Indicator for the encoding of the data field of a message.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using JetBrains.Annotations;

namespace SensateIoT.Platform.Network.Data.Enums
{
	[PublicAPI]
	public enum MessageEncoding
	{
		None,
		Base64
	}
}
