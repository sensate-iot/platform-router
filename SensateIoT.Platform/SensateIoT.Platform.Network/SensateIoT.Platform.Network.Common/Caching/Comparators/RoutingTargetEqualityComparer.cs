/*
 * Compare LiveDataRoute objects.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;

using SensateIoT.Platform.Network.Data.DTO;

namespace SensateIoT.Platform.Network.Common.Caching.Comparators
{
	public class RoutingTargetEqualityComparer : IEqualityComparer<RoutingTarget>
	{
		public bool Equals(RoutingTarget x, RoutingTarget y)
		{
			if(ReferenceEquals(x, y))
				return true;
			if(ReferenceEquals(x, null))
				return false;
			if(ReferenceEquals(y, null))
				return false;
			if(x.GetType() != y.GetType())
				return false;

			return x.Type == y.Type && x.Target == y.Target;
		}

		public int GetHashCode(RoutingTarget obj)
		{
			return HashCode.Combine(obj.Type, obj.Target);
		}
	}
}
