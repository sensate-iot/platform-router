/*
 * Measurement cache interface definition.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using SensateService.Enums;

namespace SensateService.Infrastructure.Storage
{
	public interface IMeasurementCache
	{
		Task StoreAsync(JObject obj, RequestMethod methodd);
		Task StoreRangeAsync(IEnumerable<JObject> measurements, RequestMethod method);
	}
}
