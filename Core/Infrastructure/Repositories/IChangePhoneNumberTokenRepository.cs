/*
 * Change email token repository interface.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System.Threading.Tasks;
using SensateService.Models;

namespace SensateService.Infrastructure.Repositories
{
	public interface IChangePhoneNumberTokenRepository
	{
		Task CreateAsync(ChangePhoneNumberToken token);
		Task<string> CreateAsync(SensateUser user, string token, string phonenumber);
		ChangePhoneNumberToken GetById(string id);
		ChangePhoneNumberToken GetLastByUser(SensateUser user);
	}
}