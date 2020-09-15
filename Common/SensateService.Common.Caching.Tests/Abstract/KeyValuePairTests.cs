/*
 * Unit tests for the key-value pair class.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using SensateService.Common.Caching.Abstract;
using Xunit;

namespace SensateService.Common.Caching.Tests.Abstract
{
	public class KeyValuePairTests
	{
		[Fact]
		public void CanSetKey()
		{
			var kvp = new KeyValuePair<string, string> {
				Key = "Hello, World"
			};
			Assert.Equal("Hello, World", kvp.Key);
		}

		[Fact]
		public void CanSetValue()
		{
			var kvp = new KeyValuePair<string, string> {
				Value = "KVP1"
			};

			Assert.Equal("KVP1", kvp.Value);
		}

		[Fact]
		public void KeyValuePairsAreEqual()
		{
			var kvp1 = new KeyValuePair<string, string>() {
				Key = "K1",
				Value = "V2"
			};

			var kvp2 = new KeyValuePair<string, string>() {
				Key = "K1",
				Value = "V2"
			};

			Assert.Equal(kvp1, kvp2);
		}

		[Fact]
		public void KeyValuePairsAreNotEqual()
		{
			var kvp1 = new KeyValuePair<string, string>() {
				Key = "K1",
				Value = "V3"
			};

			var kvp2 = new KeyValuePair<string, string>() {
				Key = "K1",
				Value = "V2"
			};

			Assert.NotEqual(kvp1, kvp2);
		}

		[Fact]
		public void KeyCannotBeNull()
		{
			var kvp1 = new KeyValuePair<string, string> {
				Value = "KVP 1"
			};

			var kvp2 = new KeyValuePair<string, string> {
				Value = "KVP 1"
			};

			Assert.Throws<NullReferenceException>(() => kvp1.Equals(kvp2));
		}

		[Fact]
		public void CanCompareEqual()
		{
			var kvp1 = new KeyValuePair<string, string> {
				Key = "K1",
				Value = "KVP 1"
			};

			var kvp2 = new KeyValuePair<string, string> {
				Key = "K1",
				Value = "KVP 1"
			};

			Assert.True(kvp1 == kvp2);
		}

		[Fact]
		public void CanCompareNotEqual()
		{
			var kvp1 = new KeyValuePair<string, string> {
				Key = "K1",
				Value = "KVP 1"
			};

			var kvp2 = new KeyValuePair<string, string> {
				Key = "K1",
				Value = "KVP 2"
			};

			Assert.True(kvp1 != kvp2);
		}

		[Fact]
		public void CanGenerateAHashCode()
		{
			var kvp1 = new KeyValuePair<string, string> {
				Key = "K1",
				Value = "KVP 1"
			};

			Assert.True(kvp1.GetHashCode() != default);
		}
	}
}