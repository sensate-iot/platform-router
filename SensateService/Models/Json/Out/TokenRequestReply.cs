/*
 * Request JWT token reply viewmodel.
 *
 * @author Michel Megens
 * @email   dev@bietje.net
 */

namespace SensateService.Models.Json.Out
{
	public class TokenRequestReply
	{
		public string RefreshToken { get; set; }
		public string JwtToken { get; set; }
		public int ExpiresInMinutes { get; set; }
		public int JwtExpiresInMinutes { get; set; }
	}
}
