/*
 * Garbage collection helper.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Runtime;

namespace SensateIoT.Platform.Network.Common.Caching.Routing
{
	internal static class GarbageCollection
	{
		public static void Collect()
		{
			GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;

			GC.Collect();
			GC.WaitForPendingFinalizers();
		}
	}
}
