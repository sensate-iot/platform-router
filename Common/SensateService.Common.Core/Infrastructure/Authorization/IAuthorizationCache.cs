/*
 * Authorization cache interface.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using SensateService.Common.Data.Dto.Authorization;
using SensateService.Common.Data.Dto.Generic;

namespace SensateService.Infrastructure.Authorization
{
	public interface IAuthorizationCache
	{
		void AddMeasurement(JsonMeasurement data);
		void AddMeasurements(IEnumerable<JsonMeasurement> data);
		void AddMessage(JsonMessage data);
		void AddMessages(IEnumerable<JsonMessage> data);
		Task Load();
		Task<Tuple<int, int>> ProcessAsync();
		void AddCommand(Command cmd);
		Task ProcessCommandsAsync();
	}
}
