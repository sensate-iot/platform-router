/*
 * Underflow exception implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using JetBrains.Annotations;

namespace SensateIoT.Platform.Network.Common.Exceptions
{
	[PublicAPI]
	public class UnderflowException : InvalidOperationException
	{
		public UnderflowException() : base("Unable to complete operation: underflow error.")
		{
		}

		public UnderflowException(string message) : base(message)
		{
		}

		public UnderflowException(string message, Exception inner) : base(message, inner)
		{
		}
	}
}
