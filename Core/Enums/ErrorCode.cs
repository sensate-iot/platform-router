/*
 * Error code definitions.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
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
}
