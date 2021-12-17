using System;
using System.Collections.Generic;

namespace SensateIoT.Platform.Router.Contracts.DTO
{
	public class GenericResponse<TValue>
	{
		public Guid Id { get; }
		public TValue Data { get; set; }
		public IEnumerable<string> Errors { get; set; }

		public GenericResponse()
		{
			this.Id = Guid.NewGuid();
		}
	}
}
