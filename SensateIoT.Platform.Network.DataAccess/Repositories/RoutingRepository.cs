/*
 * Account repository interface.
 * 
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

using MongoDB.Bson;
using MongoDB.Driver;

using Npgsql;
using NpgsqlTypes;

using SensateIoT.Platform.Network.Data.DTO;
using SensateIoT.Platform.Network.DataAccess.Abstract;
using SensateIoT.Platform.Network.DataAccess.Contexts;

using ApiKey = SensateIoT.Platform.Network.Data.DTO.ApiKey;
using Sensor = SensateIoT.Platform.Network.Data.DTO.Sensor;

namespace SensateIoT.Platform.Network.DataAccess.Repositories
{
	public class RoutingRepository : IRoutingRepository
	{
		private readonly IAuthorizationDbContext m_ctx;
		private readonly INetworkingDbContext m_networkContext;
		private readonly IMongoCollection<Data.Models.Sensor> m_sensors;

		private const string Router_GetAccounts = "router_getaccounts";
		private const string Router_GetAccount = "router_getaccount";
		private const string Router_GetTriggers = "router_gettriggers";
		private const string Router_GetTriggerByID = "router_gettriggersbyid";
		private const string Router_GetSensorKeys = "router_getsensorkeys";
		private const string Router_GetSensorKey = "router_getsensorkey";

		public RoutingRepository(IAuthorizationDbContext ctx, INetworkingDbContext tctx, MongoDBContext mctx)
		{
			this.m_ctx = ctx;
			this.m_networkContext = tctx;
			this.m_sensors = mctx.Sensors;
		}

		public async Task<IEnumerable<Account>> GetAccountsForRoutingAsync(CancellationToken ct)
		{
			var result = new List<Account>();

			await using var cmd = this.m_ctx.Connection.CreateCommand();
			if(cmd.Connection != null && cmd.Connection.State != ConnectionState.Open) {
				await cmd.Connection.OpenAsync(ct).ConfigureAwait(false);
			}

			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = Router_GetAccounts;

			await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
			while(await reader.ReadAsync(ct)) {
				var account = new Account {
					ID = reader.GetGuid(0),
					HasBillingLockout = reader.GetBoolean(1),
					IsBanned = reader.GetBoolean(2)
				};

				result.Add(account);
			}

			return result;
		}

		public async Task<Account> GetAccountForRoutingAsync(Guid accountId, CancellationToken ct = default)
		{
			Account result;

			await using var cmd = this.m_ctx.Connection.CreateCommand();
			if(cmd.Connection != null && cmd.Connection.State != ConnectionState.Open) {
				await cmd.Connection.OpenAsync(ct).ConfigureAwait(false);
			}

			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = Router_GetAccount;

			var param = new NpgsqlParameter("userid", NpgsqlDbType.Uuid) { Value = accountId };
			cmd.Parameters.Add(param);

			await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
			if(await reader.ReadAsync(ct).ConfigureAwait(false)) {
				result = new Account {
					ID = reader.GetGuid(0),
					HasBillingLockout = reader.GetBoolean(1),
					IsBanned = reader.GetBoolean(2)
				};
			} else {
				result = null;
			}

			return result;
		}

		public async Task<IEnumerable<Tuple<string, ApiKey>>> GetApiKeysAsync(CancellationToken ct = default)
		{
			var result = new List<Tuple<string, ApiKey>>();

			await using var cmd = this.m_ctx.Connection.CreateCommand();
			if(cmd.Connection != null && cmd.Connection.State != ConnectionState.Open) {
				await cmd.Connection.OpenAsync(ct).ConfigureAwait(false);
			}

			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = Router_GetSensorKeys;

			await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
			while(await reader.ReadAsync(ct)) {
				var key = new ApiKey {
					AccountID = reader.GetGuid(1),
					IsRevoked = reader.GetBoolean(2),
					IsReadOnly = reader.GetBoolean(3)
				};

				result.Add(new Tuple<string, ApiKey>(reader.GetString(0), key));
			}

			return result;

		}

		public async Task<ApiKey> GetApiKeyAsync(string key, CancellationToken ct = default)
		{
			ApiKey result;

			await using var cmd = this.m_ctx.Connection.CreateCommand();
			if(cmd.Connection != null && cmd.Connection.State != ConnectionState.Open) {
				await cmd.Connection.OpenAsync(ct).ConfigureAwait(false);
			}

			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = Router_GetSensorKey;

			var param = new NpgsqlParameter("sensorkey", NpgsqlDbType.Text) { Value = key };
			cmd.Parameters.Add(param);

			await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
			if(await reader.ReadAsync(ct).ConfigureAwait(false)) {
				result = new ApiKey {
					AccountID = reader.GetGuid(1),
					IsRevoked = reader.GetBoolean(2),
					IsReadOnly = reader.GetBoolean(3)
				};
			} else {
				result = null;
			}

			return result;
		}

		public async Task<IEnumerable<Sensor>> GetSensorsAsync(CancellationToken ct = default)
		{
			var result = new List<Sensor>();

			var query = this.m_sensors.Aggregate()
				.Project(new BsonDocument {
					{"SensorKey", "$Secret"},
					{"AccountID", "$Owner"},
					{"StorageEnabled", "$StorageEnabled"}
				});
			var cursor = await query.ToCursorAsync(ct).ConfigureAwait(false);

			await cursor.ForEachAsync(document => {
				var sensor = new Sensor {
					AccountID = Guid.Parse(document["AccountID"].AsString),
					ID = document["_id"].AsObjectId,
					SensorKey = document["SensorKey"].AsString,
					StorageEnabled = !document.Contains("StorageEnabled") || document["StorageEnabled"].AsBoolean
				};

				result.Add(sensor);
			}, ct).ConfigureAwait(false);

			return result;
		}

		public async Task<Sensor> GetSensorsByIDAsync(ObjectId sensorID, CancellationToken ct = default)
		{
			var query = this.m_sensors.Aggregate()
				.Match(new BsonDocument("_id", sensorID))
				.Project(new BsonDocument {
					{"SensorKey", "$Secret"},
					{"AccountID", "$Owner"},
					{"StorageEnabled", "$StorageEnabled"},
					{"_id", 1}
				});

			var result = await query.FirstOrDefaultAsync(ct).ConfigureAwait(false);

			return new Sensor {
				AccountID = Guid.Parse(result["AccountID"].AsString),
				ID = result["_id"].AsObjectId,
				SensorKey = result["SensorKey"].AsString,
				StorageEnabled = !result.Contains("StorageEnabled") || result["StorageEnabled"].AsBoolean
			};
		}

		public async Task<IEnumerable<TriggerRoutingInfo>> GetTriggerInfoAsync(ObjectId sensorID, CancellationToken ct)
		{
			var result = new List<TriggerRoutingInfo>();

			await using var cmd = this.m_networkContext.Connection.CreateCommand();
			if(cmd.Connection != null && cmd.Connection.State != ConnectionState.Open) {
				await cmd.Connection.OpenAsync(ct).ConfigureAwait(false);
			}

			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = Router_GetTriggerByID;
			cmd.Parameters.Add(new NpgsqlParameter("id", DbType.String) { Value = sensorID.ToString() });

			await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);

			while(await reader.ReadAsync(ct)) {
				var record = new TriggerRoutingInfo {
					SensorID = ObjectId.Parse(reader.GetString(0)),
					ActionCount = reader.GetInt64(1),
					TextTrigger = reader.GetBoolean(2)
				};

				result.Add(record);
			}

			return result;
		}

		public async Task<IEnumerable<TriggerRoutingInfo>> GetTriggerInfoAsync(CancellationToken ct)
		{
			var result = new List<TriggerRoutingInfo>();

			await using var cmd = this.m_networkContext.Connection.CreateCommand();
			if(cmd.Connection != null && cmd.Connection.State != ConnectionState.Open) {
				await cmd.Connection.OpenAsync(ct).ConfigureAwait(false);
			}

			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = Router_GetTriggers;

			await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
			while(await reader.ReadAsync(ct)) {
				var record = new TriggerRoutingInfo {
					SensorID = ObjectId.Parse(reader.GetString(0)),
					ActionCount = reader.GetInt64(1),
					TextTrigger = reader.GetBoolean(2)
				};

				result.Add(record);
			}

			return result;
		}
	}
}
