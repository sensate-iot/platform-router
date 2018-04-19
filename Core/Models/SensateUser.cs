/*
 * Sensate user model.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace SensateService.Models
{
	public class SensateUser : IdentityUser
	{
		[Required]
		public string FirstName { get; set; }
		[Required]
		public string LastName { get; set; }
	}
}
