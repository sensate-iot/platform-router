/*
 * Helper methods to generate random strings.
 *
 * @author Michel Megens
 * @author dev@bietje.net
 */

using System;
using System.Linq;

namespace SensateService.Helpers
{
	public static class RandomHelper
	{
		private static readonly string Characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

		public static string NextString(this Random rng, int length)
		{
			char[] ary;

			ary = Enumerable.Repeat(RandomHelper.Characters, length)
				.Select(s => s[rng.Next(0, Characters.Length)]).ToArray();
			return new string(ary);
		}
	}
}
