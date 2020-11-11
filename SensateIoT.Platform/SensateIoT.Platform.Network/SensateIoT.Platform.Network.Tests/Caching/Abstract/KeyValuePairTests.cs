/*
 * Unit tests for the key-value pair class.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SensateIoT.Platform.Network.Common.Caching.Abstract;

namespace SensateIoT.Platform.Network.Tests.Caching.Abstract
{
	[TestClass]
	public class KeyValuePairTests
	{
		[TestMethod]
		public void CanSetKey()
		{
			var kvp = new KeyValuePair<string, string> {
				Key = "Hello, World"
			};
			Assert.AreEqual("Hello, World", kvp.Key);
		}

		[TestMethod]
		public void CanSetValue()
		{
			var kvp = new KeyValuePair<string, string> {
				Value = "KVP1"
			};

			Assert.AreEqual("KVP1", kvp.Value);
		}

		[TestMethod]
		public void KeyValuePairsAreAreEqual()
		{
			var kvp1 = new KeyValuePair<string, string>() {
				Key = "K1",
				Value = "V2"
			};

			var kvp2 = new KeyValuePair<string, string>() {
				Key = "K1",
				Value = "V2"
			};

			Assert.AreEqual(kvp1, kvp2);
		}

		[TestMethod]
		public void KeyValuePairsAreNotAreEqual()
		{
			var kvp1 = new KeyValuePair<string, string>() {
				Key = "K1",
				Value = "V3"
			};

			var kvp2 = new KeyValuePair<string, string>() {
				Key = "K1",
				Value = "V2"
			};

			Assert.AreNotEqual(kvp1, kvp2);
		}

		[TestMethod]
		public void KeyCannotBeNull()
		{
			var kvp1 = new KeyValuePair<string, string> {
				Value = "KVP 1"
			};

			var kvp2 = new KeyValuePair<string, string> {
				Value = "KVP 1"
			};

			Assert.ThrowsException<NullReferenceException>(() => kvp1.Equals(kvp2));
		}

		[TestMethod]
		public void CanCompareAreEqual()
		{
			var kvp1 = new KeyValuePair<string, string> {
				Key = "K1",
				Value = "KVP 1"
			};

			var kvp2 = new KeyValuePair<string, string> {
				Key = "K1",
				Value = "KVP 1"
			};

			Assert.IsTrue(kvp1 == kvp2);
		}

		[TestMethod]
		public void CanCompareNotAreEqual()
		{
			var kvp1 = new KeyValuePair<string, string> {
				Key = "K1",
				Value = "KVP 1"
			};

			var kvp2 = new KeyValuePair<string, string> {
				Key = "K1",
				Value = "KVP 2"
			};

			Assert.IsTrue(kvp1 != kvp2);
		}
	}
}