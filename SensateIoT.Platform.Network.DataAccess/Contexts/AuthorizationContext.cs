/*
 * API key database context.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using Microsoft.EntityFrameworkCore;

namespace SensateIoT.Platform.Network.DataAccess.Contexts
{
	public class AuthorizationContext : DbContext
	{
		public AuthorizationContext(DbContextOptions<AuthorizationContext> options)
			: base(options)
		{
		}
	}
}