/*
 * Top level message router interface.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using SensateIoT.Platform.Network.Data.Abstract;

namespace SensateIoT.Platform.Router.Common.Routing.Abstract
{
	public interface IMessageRouter : IDisposable
	{
		void AddRouter(IRouter router);
		void Route(IEnumerable<IPlatformMessage> message);
	}
}
