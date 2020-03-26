/*
 * Filter parameters for API keys.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using SensateService.Enums;

namespace SensateService.AuthApi.Json
{
	public class ApiKeyFilter
	{
		public bool IncludeRevoked { get; set; }
		public IList<ApiKeyType> Types { get; set; }
		public string Query { get; set; }
		public int? Skip { get; set; }
		public int? Limit { get; set; }
	}
}