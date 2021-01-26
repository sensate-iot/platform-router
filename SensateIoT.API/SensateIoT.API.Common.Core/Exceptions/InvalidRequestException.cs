/*
 * Thrown when an invalid request has been made.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using SensateIoT.API.Common.Core.Helpers;
using SensateIoT.API.Common.Data.Enums;

namespace SensateIoT.API.Common.Core.Exceptions
{
	public class InvalidRequestException : SystemException
	{
		private int _error_code;
		public int ErrorCode { get { return this._error_code; } }

		public InvalidRequestException() : this("Invalid request received!")
		{
		}

		public InvalidRequestException(string msg) : base(msg)
		{
			this._error_code = 400;
		}

		public InvalidRequestException(int error) : this()
		{
			this._error_code = error;
		}

		public InvalidRequestException(int error, Exception inner) : base(inner.Message, inner)
		{
			this._error_code = error;
		}

		public InvalidRequestException(int error, string msg) : base(msg)
		{
			this._error_code = error;
		}

		public InvalidRequestException(ErrorCode error, string msg) : this(error.ToInt(), msg)
		{ }
	}
}
