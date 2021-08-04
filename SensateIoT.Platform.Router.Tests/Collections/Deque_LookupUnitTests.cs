/*
 * Lookup unit tests for Deque.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SensateIoT.Platform.Router.Tests.Utility;

namespace SensateIoT.Platform.Router.Tests.Collections
{
	[TestClass]
	public class Deque_LookupUnitTests
	{
		[TestMethod]
		public void BracketOperator_CanLookup()
		{
			var queue = QueueGenerationHelpers.GenerateQueue();

			Assert.AreEqual(1, queue[0]);
			Assert.AreEqual(8, queue[7]);
		}

		[TestMethod]
		public void BracketOperator_CanLookupInASplitDeque()
		{
			var queue = QueueGenerationHelpers.GenerateSplitQueue();

			Assert.AreEqual(4, queue[0]);
			Assert.AreEqual(11, queue[6]);
		}

		[TestMethod]
		public void BracketOperator_CannotLookupPastCount()
		{
			var queue = QueueGenerationHelpers.GenerateQueue();

			Assert.ThrowsException<ArgumentOutOfRangeException>(() => queue[12]);
		}

		[TestMethod]
		public void BracketOperator_CannotLookupPastCountOnASplitQueue()
		{
			var queue = QueueGenerationHelpers.GenerateSplitQueue();

			Assert.ThrowsException<ArgumentOutOfRangeException>(() => queue[7]);
		}

		[TestMethod]
		public void BracketOperator_CannotLookupPastCapacity()
		{
			var queue = QueueGenerationHelpers.GenerateQueue();

			Assert.ThrowsException<ArgumentOutOfRangeException>(() => queue[32]);
		}

		[TestMethod]
		public void BracketOperator_CannotLookupPastCapacityOnASplitQueue()
		{
			var queue = QueueGenerationHelpers.GenerateSplitQueue();

			Assert.ThrowsException<ArgumentOutOfRangeException>(() => queue[32]);
		}

		[TestMethod]
		public void Peek_BehavesLikeBracketOperator()
		{
			var queue = QueueGenerationHelpers.GenerateQueue();

			Assert.AreEqual(queue[0], queue.Peek(0));
			Assert.AreEqual(queue[7], queue.Peek(7));
			Assert.ThrowsException<ArgumentOutOfRangeException>(() => queue.Peek(12));
		}

		[TestMethod]
		public void Peek_BehavesLikeBracketOperatorOnASplitQueue()
		{
			var queue = QueueGenerationHelpers.GenerateSplitQueue();

			Assert.AreEqual(queue[0], queue.Peek(0));
			Assert.AreEqual(queue[6], queue.Peek(6));
			Assert.ThrowsException<ArgumentOutOfRangeException>(() => queue.Peek(12));
		}

		[TestMethod]
		public void Contains_CanCheckIfSplitQueueContainsValue()
		{
			var queue = QueueGenerationHelpers.GenerateSplitQueue();

			Assert.IsTrue(queue.Contains(4));
			Assert.IsTrue(queue.Contains(11));
			Assert.IsFalse(queue.Contains(200));
		}

		[TestMethod]
		public void Contains_CanCheckIfQueueContainsValue()
		{
			var queue = QueueGenerationHelpers.GenerateQueue();

			Assert.IsTrue(queue.Contains(1));
			Assert.IsTrue(queue.Contains(8));
			Assert.IsFalse(queue.Contains(200));
		}
	}
}
