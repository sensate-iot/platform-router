using System;

namespace SensateIoT.Platform.Router.Tests.Utility
{
	public static class IntegerExtensions
	{
		public static void Times(this int count, Action<int> action)
		{
			for(var i = 0; i < count; i++) {
				action(i);
			}
		}
	}
}