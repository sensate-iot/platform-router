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
using SensateService.Enums;
using SensateService.Models;

namespace SensateService.Infrastructure.Repositories
{
	public interface IMessageRepository
	{
		Task CreateAsync(Message msg, CancellationToken ct = default);
		Task CreateRangeAsync(IEnumerable<Message> messages, CancellationToken ct = default);
		Task<IEnumerable<Message>> GetAsync(Sensor sensor, int skip = 0, int take = -1,
											OrderDirection order = OrderDirection.None, CancellationToken ct = default);
		Task<IEnumerable<Message>> GetAsync(Sensor sensor, DateTime start, DateTime end, int skip = 0, int take = -1,
											OrderDirection order = OrderDirection.None, CancellationToken ct = default);

		Task<Message> GetAsync(string messageId, CancellationToken ct = default);
		Task<long> CountAsync(IList<Sensor> sensors, DateTime start, DateTime end, CancellationToken ct = default);
		Task DeleteBySensorAsync(Sensor sensor, CancellationToken ct = default);
		Task DeleteBySensorAsync(Sensor sensor, DateTime start, DateTime end, CancellationToken ct = default);
	}
}

