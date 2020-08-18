/*
 * Data point matching utility's.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using SensateService.Common.Data.Models;

namespace SensateService.Processing.TriggerHandler.Utils
{
	public class DataPointMatchUtility
	{
		public static bool MatchDatapoint(Trigger trigger, DataPoint dp)
		{
			bool rv;

			rv = false;

			if(trigger.LowerEdge != null && trigger.UpperEdge == null) {
				rv = dp.Value >= trigger.LowerEdge.Value;
			} else if(trigger.LowerEdge == null && trigger.UpperEdge != null) {
				rv = dp.Value <= trigger.UpperEdge.Value;
			} else if(trigger.LowerEdge != null && trigger.UpperEdge != null) {
				rv = dp.Value >= trigger.LowerEdge.Value && dp.Value <= trigger.UpperEdge.Value;
			}

			return rv;
		}
	}
}