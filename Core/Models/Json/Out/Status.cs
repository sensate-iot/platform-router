/*
 * Status code viewmodel.
 *
 * @author Michel Megens
 * @email   michel.megens@sonatolabs.com
 */

using SensateService.Enums;

namespace SensateService.Models.Json.Out
{
	public class Status
	{
		public ReplyCode ErrorCode { get; set; }
		public string Message { get; set; }
	}
}
