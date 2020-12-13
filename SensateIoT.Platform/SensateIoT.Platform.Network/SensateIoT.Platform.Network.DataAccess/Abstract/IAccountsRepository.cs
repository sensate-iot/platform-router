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

using SensateIoT.Platform.Network.Data.DTO;

namespace SensateIoT.Platform.Network.DataAccess.Abstract
{
	public interface IAccountsRepository
	{
		Task<IEnumerable<Account>> GetAccountsAsync(CancellationToken ct = default);
		Task<Account> GetAccountAsync(Guid accountId, CancellationToken ct = default);
		Task<IEnumerable<ApiKey>> GetApiKeysAsync(CancellationToken ct = default);
		Task<ApiKey> GetApiKeyAsync(string key, CancellationToken ct = default);
	}
}
