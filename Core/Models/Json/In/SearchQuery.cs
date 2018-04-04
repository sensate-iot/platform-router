/*
 * Forgot password JSON model.
 *
 * @author Michel Megens
 * @email   dev@bietje.net
 */

using System.ComponentModel.DataAnnotations;

namespace SensateService.Models.Json.In
{
	public class SearchQuery
	{
		[Required]
		public string Query { get; set; }
	}
}