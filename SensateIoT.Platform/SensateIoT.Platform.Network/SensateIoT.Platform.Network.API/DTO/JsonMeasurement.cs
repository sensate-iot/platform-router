using System;
using Newtonsoft.Json.Linq;

namespace SensateIoT.Platform.Network.API.DTO
{
	public class JsonMeasurement : Tuple<Measurement, string>
	{
		public JsonMeasurement(Measurement m, string t) : base(m, t)
		{
		}
	}
}
