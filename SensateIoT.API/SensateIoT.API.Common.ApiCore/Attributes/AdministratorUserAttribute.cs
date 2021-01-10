/*
 * Attribute to restrict access to the Administrators group.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using Microsoft.AspNetCore.Authorization;

namespace SensateIoT.API.Common.ApiCore.Attributes
{
	public class AdministratorUserAttribute : AuthorizeAttribute
	{
		public AdministratorUserAttribute() : base()
		{
			this.Roles = "Administrators";
		}
	}
}
