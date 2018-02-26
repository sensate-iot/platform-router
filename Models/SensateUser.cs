/*
 * Sensate user model.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Linq;

using Microsoft.IdentityModel;
using Microsoft.AspNetCore.Identity;

namespace SensateService.Models
{
	public class SensateUser : IdentityUser
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
	}
}
