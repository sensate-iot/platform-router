/*
 * Authorization cache interface.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using SensateService.Common.Data.Dto.Authorization;
using SensateService.Common.Data.Dto.Json.In;

namespace SensateService.Infrastructure.Authorization
{
	public interface IAuthorizationCache
	{
		void AddMeasurement(JsonMeasurement data);
		void AddMeasurements(IEnumerable<JsonMeasurement> data);
		Task Load();
		int Process();
	}
}
