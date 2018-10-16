/*
 * Attribute to restrict access to the Users group or higher.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using Microsoft.AspNetCore.Authorization;

namespace SensateService.Core.Api.Attributes
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
