/*
 * Status code viewmodel.
 *
 * @author Michel Megens
 * @email   michel.megens@sonatolabs.com
 */

using SensateService.Common.Data.Enums;

namespace SensateService.Common.Data.Dto.Json.Out
{
	public class Status
	{
		public ReplyCode ErrorCode { get; set; }
		public string Message { get; set; }
	}
}
