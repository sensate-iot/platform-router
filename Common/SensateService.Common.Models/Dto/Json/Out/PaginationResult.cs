/*
 * Paginatable result.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;

namespace SensateService.Common.Data.Dto.Json.Out
{
	public class PaginationResult<T>
	{
		public int Count { get; set; }
		public IEnumerable<T> Values { get; set; }
	}
}