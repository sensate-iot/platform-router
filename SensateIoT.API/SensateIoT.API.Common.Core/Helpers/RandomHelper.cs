/*
 * Helper methods to generate random strings.
 *
 * @author Michel Megens
 * @author michel.megens@sonatolabs.com
 */

using System;
using System.Linq;

namespace SensateIoT.API.Common.Core.Helpers
{
	public static class RandomHelper
	{
		private const string Characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		private const string Symbols = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@!_-";

		public static string NextString(this Random rng, int length)
		{
			char[] ary;

			ary = Enumerable.Repeat(Characters, length)
				.Select(s => s[rng.Next(0, Characters.Length)]).ToArray();
			return new string(ary);
		}

		public static string NextStringWithSymbols(this Random rng, int length)
		{
			char[] ary;

			ary = Enumerable.Repeat(Symbols, length)
				.Select(s => s[rng.Next(0, Symbols.Length)]).ToArray();
			return new string(ary);
		}
	}
}
