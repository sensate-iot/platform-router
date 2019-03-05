/*
 * Measurement store interface definition.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System.Threading.Tasks;
using SensateService.Enums;
using SensateService.Models.Json.In;

namespace SensateService.Infrastructure.Storage
{
	public interface IMeasurementStore
	{
		Task StoreAsync(RawMeasurement obj, RequestMethod method = RequestMethod.Any);
	}
}
