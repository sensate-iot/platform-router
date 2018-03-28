/*
 * Status code viewmodel.
 *
 * @author Michel Megens
 * @email   dev@bietje.net
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
