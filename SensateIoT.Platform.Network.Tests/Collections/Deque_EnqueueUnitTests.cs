/*
 * Deque unit tests.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SensateIoT.Platform.Network.Tests.Utility;
using SensateIoT.Platform.Router.Common.Collections.Local;

namespace SensateIoT.Platform.Network.Tests.Collections
{
	[TestClass]
	public class Deque_EnqueueUnitTests
	{
		[TestMethod]
		public void Add_CanAddItem()
		{
			var deque = new Deque<int> { 1 };

			Assert.AreEqual(1, deque.Count);
			Assert.AreEqual(1, deque[0]);
		}

		[TestMethod]
		public void Add_CanAddItemToASplitQueue()
		{
			var deque = QueueGenerationHelpers.GenerateSplitQueue();

			deque.Add(100);
			Assert.AreEqual(100, deque.Peek(deque.Count - 1));
		}


		[TestMethod]
		public void Add_CanAddACollectionOfItems()
		{
			var deque = new Deque<int>();
			var col = new[] { 1, 2, 3, 4 };

			deque.AddRange(col);
			Assert.AreEqual(4, deque.Count);
			Assert.AreEqual(1, deque[0]);
			Assert.AreEqual(2, deque[1]);
			Assert.AreEqual(3, deque[2]);
			Assert.AreEqual(4, deque[3]);
		}

		[TestMethod]
		public void Add_CanAddACollectionOfItemsToASplitQueue()
		{
			var deque = QueueGenerationHelpers.GenerateSplitQueue();
			var col = new[] { 20, 21, 22, 23 };

			deque.AddRange(col);
			Assert.AreEqual(23, deque.Peek(deque.Count - 1));
			Assert.AreEqual(22, deque.Peek(deque.Count - 2));
			Assert.AreEqual(21, deque.Peek(deque.Count - 3));
			Assert.AreEqual(20, deque.Peek(deque.Count - 4));
			Assert.AreEqual(11, deque.Peek(deque.Count - 5));
		}


		[TestMethod]
		public void Add_CanAddRangeToFront()
		{
			var deque = QueueGenerationHelpers.GenerateQueue();

			deque.AddFront(10);

			Assert.AreEqual(10, deque[0]);
			Assert.AreEqual(9, deque.Count);
		}

		[TestMethod]
		public void Add_CanAddRangeToFrontOfSplitQueue()
		{
			var deque = QueueGenerationHelpers.GenerateSplitQueue();

			deque.AddFront(100);

			Assert.AreEqual(100, deque[0]);
			Assert.AreEqual(8, deque.Count);
		}


		[TestMethod]
		public void Add_WillExpandIfNecessary()
		{
			var deque = new Deque<int>(2);

			Assert.AreEqual(2, deque.Capacity);
			deque.AddRange(new[] { 1, 2, 3, 4, 5 });
			Assert.AreEqual(8, deque.Capacity);
		}
	}
}
