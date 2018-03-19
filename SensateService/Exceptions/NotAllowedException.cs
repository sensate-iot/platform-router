/*
 * Unauthorized operation exception.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;

namespace SensateService.Exceptions
{
    public class NotAllowedException : SystemException
    {
		public NotAllowedException() : this("Operation not allowed!")
		{ }

		public NotAllowedException(string msg) : base(msg)
		{ }

		public NotAllowedException(string msg, Exception exception) : base(msg, exception)
		{ }
    }
}
