/*
 * Compare LiveDataHandler objects.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;

using SensateIoT.Platform.Network.Data.Models;

namespace SensateIoT.Platform.Network.Common.Caching.Comparators
{
	public class LiveDataHandlerEqualityComparer : IEqualityComparer<LiveDataHandler>
	{
		public bool Equals(LiveDataHandler x, LiveDataHandler y)
		{
			if(ReferenceEquals(x, y))
				return true;
			if(ReferenceEquals(x, null))
				return false;
			if(ReferenceEquals(y, null))
				return false;
			if(x.GetType() != y.GetType())
				return false;

			return x.Name == y.Name;
		}

		public int GetHashCode(LiveDataHandler obj)
		{
			return obj.Name != null ? obj.Name.GetHashCode() : 0;
		}
	}
}
