/*
 * Status code viewmodel.
 *
 * @author Michel Megens
 * @email   michel.megens@sonatolabs.com
 */

using SensateIoT.API.Common.Data.Enums;

namespace SensateIoT.API.Common.Data.Dto.Json.Out
{
	public class Status
	{
		public ReplyCode ErrorCode { get; set; }
		public string Message { get; set; }
	}
}
