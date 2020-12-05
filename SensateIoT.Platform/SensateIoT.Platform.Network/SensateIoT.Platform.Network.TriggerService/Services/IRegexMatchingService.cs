/*
 * Regex matching service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */
using System.Collections.Generic;
using SensateIoT.Platform.Network.Data.DTO;

namespace SensateIoT.Platform.Network.TriggerService.Services
{
	public interface IRegexMatchingService
	{
		IEnumerable<TriggerAction> Match(Message msg, IList<TriggerAction> actions);
	}
}
