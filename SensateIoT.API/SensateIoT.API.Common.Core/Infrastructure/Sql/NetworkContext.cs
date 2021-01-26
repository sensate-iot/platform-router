/*
 * Networking database context.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using Microsoft.EntityFrameworkCore;

namespace SensateIoT.API.Common.Core.Infrastructure.Sql
{
	public class NetworkContext : DbContext
	{
		public NetworkContext(DbContextOptions<NetworkContext> options) : base(options)
		{
		}
	}
}
