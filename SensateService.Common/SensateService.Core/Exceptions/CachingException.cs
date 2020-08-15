/*
 * Exeption which is thrown when database reads / writes fail.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;

namespace SensateService.Exceptions
{
	public class CachingException : SystemException
	{
		public string Key { get; private set; }

		public CachingException() : base("Cache failure")
		{
		}

		public CachingException(string msg) : base(msg)
		{ }

		public CachingException(string msg, string key) : base(msg)
		{
			this.Key = key;
		}

		public CachingException(string msg, string key, Exception inner) : base(msg, inner)
		{
			this.Key = key;
		}
	}
}
