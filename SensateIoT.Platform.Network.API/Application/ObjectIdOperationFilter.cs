using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

using JetBrains.Annotations;
using MongoDB.Bson;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SensateIoT.Platform.Network.API.Application
{
	[UsedImplicitly]
	public class ObjectIdOperationFilter : IOperationFilter
	{
		private readonly IEnumerable<string> objectIdIgnoreParameters = new[] {
			"Timestamp",
			"Machine",
			"Pid",
			"Increment",
			"CreationTime"
		};

		public ObjectIdOperationFilter(IEnumerable<string> filePaths)
		{
		}

		public void Apply(OpenApiOperation operation, OperationFilterContext context)
		{
			foreach(var p in operation.Parameters.ToList()) {
				if(!this.objectIdIgnoreParameters.Any(x => p.Name.EndsWith(x))) {
					continue;
				}

				var parameterIndex = operation.Parameters.IndexOf(p);
				operation.Parameters.Remove(p);

				var dotIndex = p.Name.LastIndexOf(".", StringComparison.Ordinal);

				if(dotIndex <= -1) {
					continue;
				}

				var idName = p.Name.Substring(0, dotIndex);
				if(operation.Parameters.All(x => x.Name != idName)) {
					operation.Parameters.Insert(parameterIndex, new OpenApiParameter() {
						Name = idName,
						Schema = new OpenApiSchema() {
							Type = "string"
						},
						Example = new OpenApiString(ObjectId.Empty.ToString()),
						In = p.In,
					});
				}
			}
		}
	}
}
