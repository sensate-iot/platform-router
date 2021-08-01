/*
 * Deque unit tests.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SensateIoT.Platform.Network.Tests.Utility;
using SensateIoT.Platform.Router.Common.Collections.Abstract;
using SensateIoT.Platform.Router.Common.Collections.Local;
using SensateIoT.Platform.Router.Common.Exceptions;

namespace SensateIoT.Platform.Network.Tests.Collections
{
	[TestClass]
	public class Deque_DequeueUnitTests
	{
		[TestMethod]
		public void Dequeue_CannotDequeueFromEmptyQueue()
		{
			IQueue<int> queue = new Deque<int>();

			Assert.ThrowsException<UnderflowException>(() => queue.Dequeue());
			Assert.AreEqual(false, queue.TryTake(out _));
		}

		[TestMethod]
		public void Dequeue_CanDequeueRangeFromEmptyQueue()
		{
			IQueue<int> queue = new Deque<int>();
			var resuls = queue.Take(10);

			Assert.AreEqual(0, queue.DequeueRange(10).Count());
			Assert.AreEqual(0, resuls.Count());
		}

		[TestMethod]
		public void DequeueRange_CanDequeueRangeFromSplitQueue()
		{
			var queue = QueueGenerationHelpers.GenerateSplitQueue();
			var values = queue.DequeueRange(6);
			var list = values.ToList();
			Assert.AreEqual(1, queue.Count);
			Assert.AreEqual(8, queue.Capacity);

			Assert.AreEqual(4, list[0]);
			Assert.AreEqual(5, list[1]);
			Assert.AreEqual(6, list[2]);
			Assert.AreEqual(7, list[3]);
			Assert.AreEqual(8, list[4]);
			Assert.AreEqual(10, list[5]);
			Assert.AreEqual(11, queue[0]);
		}

		[TestMethod]
		public void DequeueRange_CanDequeueRangeFromQueue()
		{
			IQueue<int> queue = new Deque<int>(2);
			var list = new List<int>();

			8.Times(i => list.Add(i + 1));

			queue.AddRange(list);
			var result = queue.DequeueRange(16).ToList();

			8.Times(i => {
				Assert.AreEqual(result[i], list[i]);
			});
		}

		[TestMethod]
		public void TryTake_CanTryTake()
		{
			IQueue<int> queue = new Deque<int>(2);

			8.Times(i => queue.Add(i + 1));
			queue.TryTake(out var item);

			Assert.AreEqual(1, item);
		}

		[TestMethod]
		public void TryTake_CanTryTakeFromASplitQueue()
		{
			var queue = QueueGenerationHelpers.GenerateSplitQueue();
			queue.TryTake(out var item);

			Assert.AreEqual(6, queue.Count);
			Assert.AreEqual(8, queue.Capacity);
			Assert.AreEqual(4, item);
		}


		[TestMethod]
		public void DequeueRange_CanDequeueFromSplitQueue()
		{
			var queue = QueueGenerationHelpers.GenerateSplitQueue();
			var result = queue.Dequeue();
			Assert.AreEqual(6, queue.Count);
			Assert.AreEqual(8, queue.Capacity);

			Assert.AreEqual(4, result);
		}

		[TestMethod]
		public void DequeueRange_CanDequeueFromQueue()
		{
			IQueue<int> queue = new Deque<int>(2);
			var list = new List<int>();

			8.Times(i => list.Add(i + 1));

			queue.AddRange(list);
			var result = queue.Dequeue();
			Assert.AreEqual(7, queue.Count);
			Assert.AreEqual(8, queue.Capacity);
			Assert.AreEqual(1, result);
		}

		[TestMethod]
		public void DequeueRange_TakeCountIsSameAsDequeueRange()
		{
			IQueue<int> q1 = new Deque<int>(2);
			IQueue<int> q2 = new Deque<int>(2);
			var list = new List<int>();

			8.Times(i => list.Add(i + 1));

			q1.AddRange(list);
			q2.AddRange(list);

			var r1 = q1.DequeueRange(16).ToList();
			var r2 = q2.Take(16).ToList();

			for(var idx = 0; idx < r1.Count; idx++) {
				Assert.AreEqual(r1[idx], r2[idx]);
			}

			Assert.AreEqual(0, q1.Count);
			Assert.AreEqual(8, q2.Count);
			Assert.AreEqual(q1.Capacity, q2.Capacity);
		}

		[TestMethod]
		public void DequeueRange_TakeCountIsSameAsDequeueRangeOnASplitQueue()
		{
			var q1 = QueueGenerationHelpers.GenerateSplitQueue();
			var q2 = QueueGenerationHelpers.GenerateSplitQueue();

			Assert.AreEqual(q1.Count, q2.Count);
			Assert.AreEqual(q1.Capacity, q2.Capacity);

			var r1 = q1.DequeueRange(6).ToList();
			var r2 = q2.Take(6).ToList();

			for(var idx = 0; idx < r1.Count; idx++) {
				Assert.AreEqual(r1[idx], r2[idx]);
			}

			Assert.AreEqual(1, q1.Count);
			Assert.AreEqual(8, q1.Capacity);
			Assert.AreEqual(7, q2.Count);
			Assert.AreEqual(8, q1.Capacity);
		}
	}
}
