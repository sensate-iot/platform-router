/*
 * User token data model. Table model for JWT tokens and JWT
 * refresh tokens.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SensateService.Models
{
	[Table("AuthTokens")]
	public class AuthUserToken
	{
		public bool Valid { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime ExpiresAt { get; set; }
		public string Value { get; set; }
		public string LoginProvider { get; set; }

		[ForeignKey("User")]
		public string UserId { get; set; }
		public SensateUser User { get; set; }

		public AuthUserToken()
		{
			this.Valid = true;
			this.CreatedAt = DateTime.Now;
		}

		public AuthUserToken(TimeSpan expiresIn) : this()
		{
			this.ExpiresAt = DateTime.Now.Add(expiresIn);
		}
	}
}
