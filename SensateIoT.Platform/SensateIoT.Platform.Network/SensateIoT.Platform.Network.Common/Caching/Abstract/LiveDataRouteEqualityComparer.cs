/*
 * Compare LiveDataRoute objects.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using SensateIoT.Platform.Network.Data.DTO;

namespace SensateIoT.Platform.Network.Common.Caching.Abstract
{
	public class LiveDataRouteEqualityComparer : IEqualityComparer<LiveDataRoute>
	{
		public bool Equals(LiveDataRoute x, LiveDataRoute y)
		{
			if(ReferenceEquals(x, y))
				return true;
			if(ReferenceEquals(x, null))
				return false;
			if(ReferenceEquals(y, null))
				return false;
			if(x.GetType() != y.GetType())
				return false;

			return x.Target == y.Target && x.SensorID.Equals(y.SensorID);
		}

		public int GetHashCode(LiveDataRoute obj)
		{
			return HashCode.Combine(obj.Target, obj.SensorID);
		}
	}
}