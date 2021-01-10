/*
 * Repository abstraction for the Message model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SensateIoT.API.Common.Data.Models;

namespace SensateIoT.API.Common.Core.Infrastructure.Repositories
{
	public interface IControlMessageRepository
	{
		Task CreateAsync(ControlMessage msg, CancellationToken ct = default);
		Task<IEnumerable<ControlMessage>> GetAsync(Sensor sensor, int skip = -1, int take = -1, CancellationToken ct = default);
		Task<IEnumerable<ControlMessage>> GetAsync(Sensor sensor, DateTime start, DateTime end, int skip = -1, int take = -1, CancellationToken ct = default);
		Task<long> CountAsync(IList<Sensor> sensors, DateTime start, DateTime end, int skip = -1, int limit = -1, CancellationToken ct = default);
		Task<ControlMessage> GetAsync(string messageId, CancellationToken ct = default);
		Task DeleteBySensorAsync(Sensor sensor, CancellationToken ct = default);
		Task DeleteBySensorAsync(Sensor sensor, DateTime start, DateTime end, CancellationToken ct = default);
	}
}
