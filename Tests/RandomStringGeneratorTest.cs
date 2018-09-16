
using System;
using System.Threading.Tasks;

using NUnit.Framework;

using SensateService.Helpers;

namespace SensateService.Tests
{
	[TestFixture]
	public class RandomStringGeneratorTest
	{
		private Random rng;

		[SetUp]
		public void SetUp()
		{
			this.rng = new Random();
		}

		[Test]
		public void CanGenerateRandomString()
		{
			string s1, s2;

			s1 = rng.NextString(11);
			s2 = rng.NextString(11);

			Assert.IsTrue(s1 != null);
			Assert.IsTrue(s2 != null);

			Assert.IsTrue(s1.Length == s2.Length);
			Assert.IsFalse(s1.Equals(s2));
		}
	}
}
