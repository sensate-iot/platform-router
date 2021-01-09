/*
 * Match regex against string data.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using SensateIoT.Platform.Network.Data.DTO;

namespace SensateIoT.Platform.Network.TriggerService.Services
{
	public class RegexMatchingService : IRegexMatchingService
	{
		public IEnumerable<TriggerAction> Match(Message msg, IList<TriggerAction> actions)
		{
			var results = new List<TriggerAction>();

			var regexes = actions
				.GroupBy(x => x.FormalLanguage)
				.ToDictionary(x => x.Key, x => x.ToList());

			foreach(var kvp in regexes) {
				var compiled = new Regex(kvp.Key, RegexOptions.Compiled);

				if(compiled.IsMatch(msg.Data)) {
					results.AddRange(kvp.Value);
				}
			}

			return results;
		}
	}
}
