/*
 * User roles viewmodel.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.Collections.Generic;

namespace SensateIoT.API.Common.Data.Dto.Json.Out
{
	public class UserRoles
	{
		public IList<string> Roles;
		public string Email;
	}
}
