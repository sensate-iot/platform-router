/*
 * Message type indicators.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using JetBrains.Annotations;

namespace SensateIoT.Platform.Router.Data.Abstract
{
	[PublicAPI]
	public enum ControlMessageType
	{
		Mqtt = 0,
		LiveData
	}
}
