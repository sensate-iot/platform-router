/*
 * Account repository interface.
 * 
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using SensateIoT.Platform.Network.Data.Models;

namespace SensateIoT.Platform.Network.DataAccess.Abstract
{
	public interface IAccountRepository
	{
		Task<User> GetAccountAsync(Guid accountGuid, CancellationToken ct = default);
		Task<User> GetAccountByEmailAsync(string email, CancellationToken ct = default);
		Task<IEnumerable<User>> GetAccountsAsync(IEnumerable<string> idlist, CancellationToken ct = default);
	}
}