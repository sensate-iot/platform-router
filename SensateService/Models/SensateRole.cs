/*
 * Identity role model.
 * 
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using Microsoft.AspNetCore.Identity;

namespace SensateService.Models
{
    public class SensateRole : IdentityRole
    {
		public string Description { get; set; }
    }
}
