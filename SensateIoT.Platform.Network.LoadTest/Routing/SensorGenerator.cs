/*
 * Sensor generator.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Linq;

using MongoDB.Bson;

using SensateIoT.Platform.Network.Common.Caching.Abstract;
using SensateIoT.Platform.Network.Data.DTO;

namespace SensateIoT.Platform.Network.LoadTest.Routing
{
	internal static class SensorGenerator
	{
		private const string Symbols = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@!_-";
		private const int SensorSecretLength = 32;

		public static void PopulateCache(IList<Account> accounts, Random rng, int count, IRoutingCache routes)
		{
			var keys = new List<Tuple<string, ApiKey>>();
			var sensors = new List<Sensor>();

			for(var idx = 0; idx < count; idx++) {
				var accountIdx = idx % accounts.Count;
				var account = accounts[accountIdx];

				var keyMeta = new ApiKey {
					AccountID = account.ID,
					IsReadOnly = false,
					IsRevoked = false
				};
				var key = NextStringWithSymbols(rng, SensorSecretLength);

				var sensor = new Sensor {
					AccountID = account.ID,
					ID = ObjectId.GenerateNewId(),
					LiveDataRouting = null,
					SensorKey = key,
					StorageEnabled = true,
					TriggerInformation = null
				};

				sensors.Add(sensor);
				keys.Add(new Tuple<string, ApiKey>(key, keyMeta));
			}

			routes.Load(sensors);
			routes.Load(keys);
			routes.Load(accounts);
		}

		public static IEnumerable<Account> GenerateAccounts(int count)
		{
			var accounts = new List<Account>();

			for(var idx = 0; idx < count; idx++) {
				accounts.Add(new Account {
					HasBillingLockout = false,
					ID = Guid.NewGuid(),
					IsBanned = false
				});
			}

			return accounts;
		}

		private static string NextStringWithSymbols(Random rng, int length)
		{
			char[] ary;

			ary = Enumerable.Repeat(Symbols, length)
				.Select(s => s[rng.Next(0, Symbols.Length)]).ToArray();
			return new string(ary);
		}

	}
}