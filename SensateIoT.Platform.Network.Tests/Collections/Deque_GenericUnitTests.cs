/*
 * Deque unit tests.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SensateIoT.Platform.Network.Common.Collections.Abstract;
using SensateIoT.Platform.Network.Common.Collections.Local;
using SensateIoT.Platform.Network.Tests.Utility;

namespace SensateIoT.Platform.Network.Tests.Collections
{
	[TestClass]
	public class Deque_GenericUnitTests
	{
		[TestMethod]
		public void Construct_CanDefaultConstruct()
		{
			var queue = new Deque<int>();

			Assert.AreEqual(256, queue.Capacity);
			Assert.AreEqual(0, queue.Count);
		}

		[TestMethod]
		public void Construct_CanConstructWithCapacity()
		{
			var queue = new Deque<int>(50);

			Assert.AreEqual(64, queue.Capacity);
			Assert.AreEqual(0, queue.Count);
		}

		[TestMethod]
		public void Construct_CanConstructFromEnumerable()
		{
			var queue = new Deque<int>(new[] { 1, 2, 3, 4, 5 });

			Assert.AreEqual(8, queue.Capacity);
			Assert.AreEqual(5, queue.Count);
		}

		[TestMethod]
		public void Copy_CanCopyToAnArray()
		{
			var queue = QueueGenerationHelpers.GenerateQueue();
			var array = new int[queue.Count];

			queue.CopyTo(array, 0);

			foreach(var item in queue) {
				Assert.IsTrue(array.Contains(item));
			}

			Assert.AreEqual(array[0], queue[0]);
		}

		[TestMethod]
		public void Copy_CanCopyIntoArrayWrapper()
		{
			var queue = QueueGenerationHelpers.GenerateQueue();
			var array = new int[queue.Count];
			var wrapper = (Array)array;

			queue.CopyTo(wrapper, 0);
			array = wrapper.Cast<int>().ToArray();

			foreach(var item in queue) {
				Assert.IsTrue(array.Contains(item));
			}

			Assert.AreEqual(array[0], queue[0]);
		}

		[TestMethod]
		public void Copy_CanConvertToArray()
		{
			var queue = QueueGenerationHelpers.GenerateQueue();
			var array = queue.ToArray();

			foreach(var item in queue) {
				Assert.IsTrue(array.Contains(item));
			}
		}

		[TestMethod]
		public void Copy_CanCopyToAnArrayWithOffset()
		{
			var queue = QueueGenerationHelpers.GenerateQueue();
			var array = new int[queue.Count + 5];

			queue.CopyTo(array, 4);

			foreach(var item in queue) {
				Assert.IsTrue(array.Contains(item));
			}

			Assert.AreEqual(array[4], queue[0]);
		}

		[TestMethod]
		public void Copy_CanCopyIntoArrayWrapperWithOffset()
		{
			var queue = QueueGenerationHelpers.GenerateQueue();
			var array = new int[queue.Count + 5];
			var wrapper = (Array)array;

			queue.CopyTo(wrapper, 4);
			array = wrapper.Cast<int>().ToArray();

			foreach(var item in queue) {
				Assert.IsTrue(array.Contains(item));
			}

			Assert.AreEqual(array[4], queue[0]);
		}

		[TestMethod]
		public void Copy_CannotCopyToAnArrayWithIncorrectOffset()
		{
			var queue = QueueGenerationHelpers.GenerateQueue();
			var array = new int[queue.Count];

			Assert.ThrowsException<ArgumentException>(() => queue.CopyTo(array, 12));
			Assert.ThrowsException<ArgumentOutOfRangeException>(() => queue.CopyTo(array, -12));
		}

		[TestMethod]
		public void Copy_CannotCopyIntoArrayWrapperWithIncorrectOffset()
		{
			var queue = QueueGenerationHelpers.GenerateQueue();
			var array = new int[queue.Count];
			var wrapper = (Array)array;

			Assert.ThrowsException<ArgumentException>(() => queue.CopyTo(wrapper, 12));
			Assert.ThrowsException<ArgumentOutOfRangeException>(() => queue.CopyTo(wrapper, -1));
		}

		[TestMethod]
		public void Copy_CanCopyToAnArrayWhenSplit()
		{
			var queue = QueueGenerationHelpers.GenerateSplitQueue();
			var array = new int[queue.Count];

			queue.CopyTo(array, 0);

			foreach(var item in queue) {
				Assert.IsTrue(array.Contains(item));
			}

			Assert.AreEqual(array[0], queue[0]);
		}

		[TestMethod]
		public void Copy_CanCopyIntoArrayWrapperWhenSplit()
		{
			var queue = QueueGenerationHelpers.GenerateSplitQueue();
			var array = new int[queue.Count];
			var wrapper = (Array)array;

			queue.CopyTo(wrapper, 0);
			array = wrapper.Cast<int>().ToArray();

			foreach(var item in queue) {
				Assert.IsTrue(array.Contains(item));
			}

			Assert.AreEqual(array[0], queue[0]);
		}

		[TestMethod]
		public void Copy_CanConvertToArrayWhenSplit()
		{
			var queue = QueueGenerationHelpers.GenerateSplitQueue();
			var array = queue.ToArray();

			foreach(var item in queue) {
				Assert.IsTrue(array.Contains(item));
			}

			Assert.AreEqual(array[0], queue[0]);
		}

		[TestMethod]
		public void Iterate_CanLoopWithForeach()
		{
			IQueue<int> queue = new Deque<int>();

			queue.AddRange(new[] { 1, 2, 3, 4, 5 });
			var idx = 1;

			foreach(var item in queue) {
				Assert.AreEqual(item, idx);
				idx++;
			}
		}

		[TestMethod]
		public void ToArray_CanConvertToArray()
		{
			IQueue<int> queue = new Deque<int> { 1, 2, 3, 4, 5 };
			var array = queue.ToArray();
			var value = 1;

			foreach(var item in array) {
				Assert.AreEqual(item, value);
				value++;
			}
		}

		[TestMethod]
		public void Capacity_CanGrow()
		{
			IQueue<int> deque = new Deque<int>(new[] { 1, 2, 3 });

			Assert.AreEqual(4, deque.Capacity);
			deque.Capacity = 121;
			Assert.AreEqual(128, deque.Capacity);
		}

		[TestMethod]
		public void Capacity_CanGrowWhenSplit()
		{
			var deque = new Deque<int>(new[] { 1, 2, 3 });
			deque.Dequeue();
			deque.Add(4);
			Assert.AreEqual(4, deque.Capacity);
			deque.Capacity = 7;
			Assert.AreEqual(8, deque.Capacity);
			var shouldBeEmpty = deque.Except(new[] { 2, 3, 4 });
			Assert.AreEqual(0, shouldBeEmpty.Count());
		}

		[TestMethod]
		public void Capacity_CanShinkAQueue()
		{
			var queue = QueueGenerationHelpers.GenerateSplitQueue();

			queue.DequeueRange(4);
			queue.Capacity = 4;
			Assert.AreEqual(4, queue.Capacity);
			Assert.AreEqual(3, queue.Count);

		}

		[TestMethod]
		public void Capacity_CannotSetCapacityToZero()
		{
			IQueue<int> deque = new Deque<int>();

			Assert.ThrowsException<ArgumentOutOfRangeException>(() => deque.Capacity = 0);
			Assert.AreEqual(256, deque.Capacity);
		}

		[TestMethod]
		public void Capacity_CanTrim()
		{
			var queue = new Deque<int>(32);

			9.Times((i) => queue.Add(i + 1));
			Assert.AreEqual(32, queue.Capacity);
			Assert.AreEqual(9, queue.Count);
			queue.Trim();
			Assert.AreEqual(16, queue.Capacity);
			Assert.AreEqual(9, queue.Count);
		}

		[TestMethod]
		public void Capacity_CannotTrimFullqueue()
		{
			var queue = new Deque<int>(16);

			9.Times((i) => queue.Add(i + 1));
			Assert.AreEqual(16, queue.Capacity);
			Assert.AreEqual(9, queue.Count);
			queue.Trim();
			Assert.AreEqual(16, queue.Capacity);
			Assert.AreEqual(9, queue.Count);
		}

		[TestMethod]
		public void Capacity_CanTrimASplitDequeue()
		{
			var queue = QueueGenerationHelpers.GenerateSplitQueue();

			queue.DequeueRange(4);
			queue.Trim();
			Assert.AreEqual(4, queue.Capacity);
			Assert.AreEqual(3, queue.Count);
		}


		[TestMethod]
		public void Clear_CanClearAQueue()
		{
			var queue = QueueGenerationHelpers.GenerateQueue();

			queue.Clear();
			Assert.AreEqual(0, queue.Count);
			Assert.AreEqual(256, queue.Capacity);
		}
	}
}
