/*
 * user service interface for DI.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading;
using System.Threading.Tasks;
using SensateService.Models;

namespace SensateService.Services
{
	public interface IUserService
	{
		Task DeleteAsync(SensateUser user, CancellationToken ct = default);
	}
}