/*
 * User token data model. Table model for JWT tokens and JWT
 * refresh tokens.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using Microsoft.AspNetCore.Identity;

namespace SensateService.Models
{
	public class SensateUserToken : IdentityUserToken<string>
	{
		public bool Valid { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime ExpiresAt { get; set; }

		public SensateUserToken()
		{
			this.Valid = true;
			this.CreatedAt = DateTime.Now;
		}

		public SensateUserToken(TimeSpan expiresIn) : this()
		{
			this.ExpiresAt = DateTime.Now.Add(expiresIn);
		}
	}
}
