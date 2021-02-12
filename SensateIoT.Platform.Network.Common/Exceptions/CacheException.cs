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

		public CacheException()
		{ }

		public CacheException(string message) : base(message)
		{ }

		public CacheException(object key) : base($"Generic cache exception (key: {key}).")
		{
			this.Key = key;
		}

		public CacheException(object key, string message) : base(message)
		{
			this.Key = key;
		}

		public CacheException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
