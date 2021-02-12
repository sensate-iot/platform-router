/*
 * Message queueing interface.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using SensateIoT.Platform.Network.Data.Abstract;

namespace SensateIoT.Platform.Network.Common.Collections.Abstract
{
	public interface IMessageQueue : IQueue<IPlatformMessage>
	{
		TimeSpan DeltaAge();
		TimeSpan TopMedianDeltaAge();
	}
}
