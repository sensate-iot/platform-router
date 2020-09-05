/*
 * Measurement authorization controller.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using SensateService.Common.Data.Dto.Json.Out;

namespace SensateService.Processing.DataAuthorizationApi.Dto
{
	public class AuthorizationResponse : Status
	{
		public int Queued { get; set; }
		public int Rejected { get; set; }
	}
}
