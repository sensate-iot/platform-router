/*
 * Helper function to generate specific queue structures to
 * simplify unit testing.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SensateIoT.Platform.Network.Common.Collections.Abstract;
using SensateIoT.Platform.Network.Common.Collections.Local;

namespace SensateIoT.Platform.Network.Tests.Utility
{
	public static class QueueGenerationHelpers
	{
		public static IQueue<int> GenerateQueue()
		{
			IQueue<int> queue = new Deque<int>(16);

			8.Times(i => queue.Add(i + 1));
			Assert.AreEqual(queue.Count, 8);
			Assert.AreEqual(queue.Capacity, 16);

			return queue;
		}

		public static IQueue<int> GenerateSplitQueue()
		{
			IQueue<int> queue = new Deque<int>(2);

			8.Times(i => queue.Add(i + 1));
			queue.Dequeue();
			queue.Dequeue();
			queue.Dequeue();
			queue.Add(10);
			queue.Add(11);

			Assert.AreEqual(queue.Count, 7);
			Assert.AreEqual(queue.Capacity, 8);

			return queue;
		}
	}
}
