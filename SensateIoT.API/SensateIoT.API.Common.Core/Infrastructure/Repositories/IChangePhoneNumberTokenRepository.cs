/*
 * Change email token repository interface.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.Threading;
using System.Threading.Tasks;
using SensateIoT.API.Common.Data.Models;
using SensateIoT.API.Common.IdentityData.Models;

namespace SensateIoT.API.Common.Core.Infrastructure.Repositories
{
	public interface IChangePhoneNumberTokenRepository
	{
		Task CreateAsync(ChangePhoneNumberToken token, CancellationToken ct = default);
		Task<string> CreateAsync(SensateUser user, string token, string phonenumber);
		ChangePhoneNumberToken GetById(string id);
		Task<ChangePhoneNumberToken> GetLatest(SensateUser user);
	}
}