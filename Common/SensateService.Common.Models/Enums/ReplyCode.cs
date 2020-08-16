/*
 * Reply status codes.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

namespace SensateService.Common.Data.Enums
{
	public enum ReplyCode
	{
		BadInput = 400,
		NotAllowed = 401,
		NotFound = 402,
		Banned = 403,
		BillingLockout = 404,
		UnknownError = 500,
		Ok = 200
	}
}
