/*
 * Reply status codes.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

namespace SensateService.Enums
{
	public enum ReplyCode : int
	{
		BadInput = 400,
		NotAllowed = 401,
		NotFound = 402,
		Banned = 403,
		UnknownError = 500,
		Ok = 200
	}
}
