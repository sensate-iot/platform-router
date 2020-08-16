using System;

namespace SensateService.Helpers
{
	public static class StaticRandom
	{
		private static readonly Random Rng = new Random((int)DateTime.UtcNow.Ticks);
		private static readonly object Lock = new object();

		public static int Next()
		{
			lock(Lock) {
				return Rng.Next();
			}
		}

		public static int Next(int max)
		{
			lock(Lock) {
				return Rng.Next(max);
			}
		}

		public static int Next(int min, int max)
		{
			lock(Lock) {
				return Rng.Next(min, max);
			}
		}

		public static double NextDouble()
		{
			lock(Lock) {
				return Rng.NextDouble();
			}
		}

		public static void NextBytes(byte[] buffer)
		{
			lock(Lock) {
				Rng.NextBytes(buffer);
			}
		}
	}
}