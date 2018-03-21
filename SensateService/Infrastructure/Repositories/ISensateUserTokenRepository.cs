/*
 * Repository for the SensateUserToken table.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using SensateService.Models;

namespace SensateService.Infrastructure.Repositories
{
	public interface ISensateUserTokenRepository
	{
		void Create(SensateUserToken token);
		Task CreateAsync(SensateUserToken token);

		IEnumerable<SensateUserToken> GetByUser(SensateUser user);
		SensateUserToken GetById(SensateUser user, string value);
		SensateUserToken GetById(Tuple<SensateUser, string> id);
	}
}
