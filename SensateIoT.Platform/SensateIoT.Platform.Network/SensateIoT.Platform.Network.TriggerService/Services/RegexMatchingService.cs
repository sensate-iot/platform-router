/*
 * Match regex against string data.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using SensateIoT.Platform.Network.Data.Abstract;
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

			var data = msg.Data;

			if(msg.Encoding == MessageEncoding.Base64) {
				var bytes = Convert.FromBase64String(data);
				data = Encoding.UTF8.GetString(bytes);
			}

			foreach(var kvp in regexes) {
				var compiled = new Regex(kvp.Key, RegexOptions.Compiled);

				if(compiled.IsMatch(data)) {
					results.AddRange(kvp.Value);
				}
			}

			return results;
		}
	}
}
