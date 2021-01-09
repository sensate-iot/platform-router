/*
 * Paginatable result.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;

namespace SensateIoT.Platform.Network.API.DTO
{
	public class PaginationResult<T>
	{
		public int Count { get; set; }
		public int Limit { get; set; }
		public int Skip { get; set; }
		public IEnumerable<T> Values { get; set; }
	}
}
