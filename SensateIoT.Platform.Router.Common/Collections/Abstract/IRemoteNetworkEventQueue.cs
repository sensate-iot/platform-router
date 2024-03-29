﻿/*
 * Remote price update queue interface.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading.Tasks;
using SensateIoT.Platform.Router.Contracts.DTO;

namespace SensateIoT.Platform.Router.Common.Collections.Abstract
{
	public interface IRemoteNetworkEventQueue
	{
		void EnqueueEvent(NetworkEvent netEvent);
		Task FlushEventsAsync();
	}
}
