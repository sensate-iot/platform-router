/*
 * Exception for cache specific errors.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;

namespace SensateIoT.Platform.Network.Common.Exceptions
{
	public class CacheException : Exception
	{
		public object Key { get; }

		public CacheException(object key, string message) : base(message)
		{
			this.Key = key;
		}
	}
}
