/*
 * Set user role model.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System.ComponentModel.DataAnnotations;

namespace SensateService.Models.Json.In
{
	public class SetRole
	{
		[Required]
		public string UserId;
		[Required]
		public string Role;
	}
}