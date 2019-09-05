/*
 * Attribute to restrict access to the Users group or higher.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using Microsoft.AspNetCore.Authorization;

namespace SensateService.ApiCore.Attributes
{
	public class NormalUserAttribute : AuthorizeAttribute
	{
		public NormalUserAttribute() : this(true)
		{
		}

		public NormalUserAttribute(bool andHigher) : base()
		{
			string roles = "Users";

			if(andHigher) {
				roles += ",Administrators";
			}

			this.Roles = roles;
		}
	}
}
