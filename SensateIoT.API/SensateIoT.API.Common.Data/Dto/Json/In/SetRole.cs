/*
 * Set user role model.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.ComponentModel.DataAnnotations;

namespace SensateIoT.API.Common.Data.Dto.Json.In
{
	public class SetRole
	{
		[Required]
		public string UserId;
		[Required]
		public string Role;
	}
}