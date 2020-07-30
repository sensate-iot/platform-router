/*
 * Measurement cache interface definition.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Threading.Tasks;

using SensateService.Enums;

namespace SensateService.Infrastructure.Storage
{
	public interface IMeasurementCache
	{
		Task StoreAsync(string obj, RequestMethod methodd);
	}
}
