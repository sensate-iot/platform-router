/*
 * Measurement data point matcher.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using SensateIoT.Platform.Network.Data.DTO;

using TriggerAction = SensateIoT.Platform.Network.Data.DTO.TriggerAction;

namespace SensateIoT.Platform.Network.TriggerService.Abstract
{
	public interface IDataPointMatchingService
	{
		IEnumerable<TriggerAction> Match(string key, DataPoint dp, IList<TriggerAction> actions);
	}
}
