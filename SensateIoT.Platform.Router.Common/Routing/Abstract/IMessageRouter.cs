/*
 * Top level message router interface.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;

namespace SensateIoT.Platform.Router.Common.Routing.Abstract
{
	public interface IMessageRouter : IDisposable
	{
		void AddRouter(IRouter router);
		bool TryRoute();
	}
}
