/*
 * RESTful account controller.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;

namespace SensateService.Controllers
{
	public class UserAccountSettings
	{
		public string JwtKey { get; set; }
		public string JwtIssuer { get; set; }
		public int JwtExpireDays { get; set; }
	}
}
