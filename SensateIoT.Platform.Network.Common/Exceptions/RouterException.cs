using System;

namespace SensateIoT.Platform.Network.Common.Exceptions
{
	public class RouterException : InvalidOperationException
	{
		public RouterException(string router, string message) : base($"Unable to route message through {router}: {message}")
		{
		}
	}
}
