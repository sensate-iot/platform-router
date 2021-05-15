using System;

namespace SensateIoT.Platform.Network.Common.Exceptions
{
	public class RouterException : InvalidOperationException
	{
		public string Router { get; set; }

		public RouterException(string router) : base($"Unable to route message through {router}")
		{
		}

		public RouterException(string router, string message) : base($"Unable to route message through {router}: {message}")
		{
		}
	}
}
