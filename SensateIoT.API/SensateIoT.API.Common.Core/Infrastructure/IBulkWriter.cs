/*
 * Database bulk writer interface.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SensateIoT.API.Common.Core.Infrastructure
{
	public interface IBulkWriter<in T>
	{
		Task CreateRangeAsync(IEnumerable<T> objs, CancellationToken token = default);
	}
}