/*
 * Authentication options for Email services
 * 
 * @author Michel Megens
 * @email  dev@bietje.net
 */

namespace SensateService.Services
{
    public class MessageSenderAuthOptions
    {
		public string Key { get; set; }
		public string Username { get; set; }
		public string From { get; set; }
		public string FromName { get; set; }
    }
}
