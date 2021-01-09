/*
 * Pagination response type.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;

namespace SensateIoT.Platform.Network.API.DTO
{
	public class PaginationResponse<TValue>
	{
		public Guid ResponseId { get; set; }
		public IList<string> Errors { get; set; }
		public PaginationResult<TValue> Data { get; set; }

		public PaginationResponse()
		{
			this.ResponseId = Guid.NewGuid();
		}

		public void AddError(string error)
		{
			this.Errors ??= new List<string>();
			this.Errors.Add(error);
		}

	}
}