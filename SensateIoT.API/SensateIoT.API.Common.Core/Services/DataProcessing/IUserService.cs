/*
 * user service interface for DI.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading;
using System.Threading.Tasks;
using SensateIoT.API.Common.IdentityData.Models;

namespace SensateIoT.API.Common.Core.Services.DataProcessing
{
	public interface IUserService
	{
		Task DeleteAsync(SensateUser user, CancellationToken ct = default);
	}
}