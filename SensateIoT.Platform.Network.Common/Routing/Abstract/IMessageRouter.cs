/*
 * Top level message router interface.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using SensateIoT.Platform.Network.Data.Abstract;

namespace SensateIoT.Platform.Network.Common.Routing.Abstract
{
	public interface IMessageRouter : IDisposable
	{
		void AddRouter(IRouter router);
		void Route(IPlatformMessage message);
	}
}
