/*
 * Database bulk writer interface.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SensateService.Infrastructure
{
	public interface IBulkWriter<in T>
	{
		Task CreateRangeAsync(IEnumerable<T> objs, CancellationToken token = default(CancellationToken));
	}
}