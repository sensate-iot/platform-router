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
using SensateService.Models;

namespace SensateService.Infrastructure.Repositories
{
	public interface IControlMessageRepository 
	{
		Task CreateAsync(ControlMessage msg, CancellationToken ct = default);
		Task UpdateAsync(ControlMessage msg, string newmsg,  CancellationToken ct = default);
		Task DeleteAsync(ControlMessage msg, CancellationToken ct = default);
		Task<IEnumerable<ControlMessage>> GetAsync(Sensor sensor, int skip = 0, int take = -1, CancellationToken ct = default);
		Task<IEnumerable<ControlMessage>> GetAsync(Sensor sensor, DateTime start, DateTime end, int skip = 0, int take = -1, CancellationToken ct = default);
		Task<Message> GetAsync(string messageId, CancellationToken ct = default);
	}
}
