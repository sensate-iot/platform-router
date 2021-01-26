/*
 * JSON measurement.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;

namespace SensateIoT.Platform.Network.API.DTO
{
	public class JsonMeasurement : Tuple<Measurement, string>
	{
		public JsonMeasurement(Measurement m, string t) : base(m, t)
		{
		}
	}
}
