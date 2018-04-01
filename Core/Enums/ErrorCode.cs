/*
 * Error code definitions.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

namespace SensateService.Enums
{
	public enum ErrorCode : int
	{
		JsonError = 300,
		IncorrectSecretError = 301,
		InvalidDataError = 302,

		ServerFaultGeneric = 500,
		ServerFaultBadGateway = 502,
		ServerFaultUnavailable = 503
	}

	public static class Error
	{
		public const int JsonError = (int)ErrorCode.JsonError;
		public const int IncorrectSecretError = (int)ErrorCode.IncorrectSecretError;
		public const int InvalidDataError = (int)ErrorCode.InvalidDataError;

		public const int ServerFaultGeneric = (int)ErrorCode.ServerFaultGeneric;
		public const int ServerFaultBadGateway = (int)ErrorCode.ServerFaultBadGateway;
		public const int ServerFaultUnavailable = (int)ErrorCode.ServerFaultUnavailable;

	}
} 
