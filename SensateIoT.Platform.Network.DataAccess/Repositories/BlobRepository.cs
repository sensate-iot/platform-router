/*
 * Blob repository implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MongoDB.Bson;
using NpgsqlTypes;

using SensateIoT.Platform.Network.Data.Models;
using SensateIoT.Platform.Network.DataAccess.Abstract;
using SensateIoT.Platform.Network.DataAccess.Extensions;

namespace SensateIoT.Platform.Network.DataAccess.Repositories
{
	public class BlobRepository : IBlobRepository
	{
		private readonly INetworkingDbContext m_ctx;

		private const string DeleteBlobBySensorID = "networkapi_deleteblobsbysensorid";
		private const string SelectBlobByID = "networkapi_selectblobbyid";
		private const string SelectBlobByName = "networkapi_selectblobbyname";
		private const string SelectBlobs = "networkapi_selectblobs";
		private const string SelectBlobsBySensorID = "networkapi_selectblobsbysensorid";
		private const string DeleteBlobByID = "networkapi_deleteblobbyid";
		private const string DeleteBlobByName = "networkapi_deleteblobsbyname";
		private const string CreateBlob = "networkapi_createblob";

		public BlobRepository(INetworkingDbContext ctx)
		{
			this.m_ctx = ctx;
		}

		public async Task DeleteAsync(ObjectId sensor, CancellationToken ct = default)
		{
			using var builder = StoredProcedureBuilder.Create(this.m_ctx.Connection);

			builder.WithFunction(DeleteBlobBySensorID);
			builder.WithParameter("sensorid", sensor.ToString(), NpgsqlDbType.Varchar);
			var result = await builder.ExecuteAsync(ct).ConfigureAwait(false);
			await result.DisposeAsync();
		}

		public async Task<Blob> CreateAsync(Blob blob, CancellationToken ct)
		{
			using var builder = StoredProcedureBuilder.Create(this.m_ctx.Connection);

			builder.WithFunction(CreateBlob);
			builder.WithParameter("sensorid", blob.SensorID, NpgsqlDbType.Varchar);
			builder.WithParameter("filename", blob.FileName, NpgsqlDbType.Text);
			builder.WithParameter("path", blob.Path, NpgsqlDbType.Text);
			builder.WithParameter("storage", (int)blob.StorageType, NpgsqlDbType.Integer);
			builder.WithParameter("filesize", blob.FileSize, NpgsqlDbType.Integer);
			await using var reader = await builder.ExecuteAsync(ct).ConfigureAwait(false);

			if(!await reader.ReadAsync(ct).ConfigureAwait(false)) {
				return null;
			}

			return new Blob {
				ID = reader.GetInt64(0),
				SensorID = reader.GetString(1),
				FileName = reader.GetString(2),
				Path = reader.GetString(3),
				StorageType = (StorageType)reader.GetInt32(4),
				Timestamp = reader.GetDateTime(5),
				FileSize = reader.GetInt32(6)
			};
		}

		public async Task<Blob> GetAsync(long blobId, CancellationToken ct = default)
		{
			using var builder = StoredProcedureBuilder.Create(this.m_ctx.Connection);

			builder.WithFunction(SelectBlobByID);
			builder.WithParameter("id", blobId, NpgsqlDbType.Bigint);

			await using var reader = await builder.ExecuteAsync(ct).ConfigureAwait(false);

			if(!await reader.ReadAsync(ct).ConfigureAwait(false)) {
				return null;
			}

			return new Blob {
				ID = reader.GetInt64(0),
				SensorID = reader.GetString(1),
				FileName = reader.GetString(2),
				Path = reader.GetString(3),
				StorageType = (StorageType)reader.GetInt32(4),
				Timestamp = reader.GetDateTime(5),
				FileSize = reader.GetInt32(6)
			};
		}

		public async Task<IEnumerable<Blob>> GetRangeAsync(IList<Sensor> sensors, int skip = -1, int limit = -1, CancellationToken ct = default)
		{
			using var builder = StoredProcedureBuilder.Create(this.m_ctx.Connection);

			builder.WithFunction(SelectBlobs);
			var sensorids = sensors.Select(x => x.InternalId.ToString());
			var idlist = string.Join(',', sensorids);

			builder.WithParameter("idlist", idlist, NpgsqlDbType.Text);

			builder.WithParameter("offst", GetNullableInteger(skip), NpgsqlDbType.Integer);
			builder.WithParameter("lim", GetNullableInteger(limit), NpgsqlDbType.Integer);

			await using var reader = await builder.ExecuteAsync(ct).ConfigureAwait(false);
			var list = new List<Blob>();

			while(await reader.ReadAsync(ct).ConfigureAwait(false)) {
				var blob = new Blob {
					ID = reader.GetInt64(0),
					SensorID = reader.GetString(1),
					FileName = reader.GetString(2),
					Path = reader.GetString(3),
					StorageType = (StorageType)reader.GetInt32(4),
					Timestamp = reader.GetDateTime(5),
					FileSize = reader.GetInt32(6)
				};

				list.Add(blob);
			}

			return list;
		}

		private static int? GetNullableInteger(int value)
		{
			if(value < 0) {
				return null;
			}

			return value;
		}

		public async Task<Blob> GetAsync(string sensorId, string fileName, CancellationToken ct = default)
		{
			using var builder = StoredProcedureBuilder.Create(this.m_ctx.Connection);

			builder.WithFunction(SelectBlobByName);
			builder.WithParameter("sensorid", sensorId, NpgsqlDbType.Varchar);
			builder.WithParameter("filename", fileName, NpgsqlDbType.Text);

			await using var reader = await builder.ExecuteAsync(ct).ConfigureAwait(false);

			if(!await reader.ReadAsync(ct).ConfigureAwait(false)) {
				return null;
			}

			return new Blob {
				ID = reader.GetInt64(0),
				SensorID = reader.GetString(1),
				FileName = reader.GetString(2),
				Path = reader.GetString(3),
				StorageType = (StorageType)reader.GetInt32(4),
				Timestamp = reader.GetDateTime(5),
				FileSize = reader.GetInt32(6)
			};
		}

		public async Task<IEnumerable<Blob>> GetAsync(string sensorId, int skip = -1, int limit = -1, CancellationToken ct = default)
		{
			using var builder = StoredProcedureBuilder.Create(this.m_ctx.Connection);

			builder.WithFunction(SelectBlobsBySensorID);

			builder.WithParameter("sensorid", sensorId, NpgsqlDbType.Varchar);
			builder.WithParameter("offst", GetNullableInteger(skip), NpgsqlDbType.Integer);
			builder.WithParameter("lim", GetNullableInteger(limit), NpgsqlDbType.Integer);

			await using var reader = await builder.ExecuteAsync(ct).ConfigureAwait(false);
			var list = new List<Blob>();

			while(await reader.ReadAsync(ct).ConfigureAwait(false)) {
				var blob = new Blob {
					ID = reader.GetInt64(0),
					SensorID = reader.GetString(1),
					FileName = reader.GetString(2),
					Path = reader.GetString(3),
					StorageType = (StorageType)reader.GetInt32(4),
					Timestamp = reader.GetDateTime(5),
					FileSize = reader.GetInt32(6)
				};

				list.Add(blob);
			}

			return list;
		}

		public async Task<Blob> DeleteAsync(string sensorId, string fileName, CancellationToken ct = default)
		{
			using var builder = StoredProcedureBuilder.Create(this.m_ctx.Connection);

			builder.WithFunction(DeleteBlobByName);
			builder.WithParameter("sensorid", sensorId, NpgsqlDbType.Varchar);
			builder.WithParameter("filename", fileName, NpgsqlDbType.Text);
			await using var reader = await builder.ExecuteAsync(ct).ConfigureAwait(false);

			if(!await reader.ReadAsync(ct).ConfigureAwait(false)) {
				return null;
			}

			return new Blob {
				ID = reader.GetInt64(0),
				SensorID = reader.GetString(1),
				FileName = reader.GetString(2),
				Path = reader.GetString(3),
				StorageType = (StorageType)reader.GetInt32(4),
				Timestamp = reader.GetDateTime(5),
				FileSize = reader.GetInt32(6)
			};
		}

		public async Task<bool> DeleteAsync(long id, CancellationToken ct = default)
		{
			using var builder = StoredProcedureBuilder.Create(this.m_ctx.Connection);

			builder.WithFunction(DeleteBlobByID);
			builder.WithParameter("id", id, NpgsqlDbType.Bigint);
			await using var result = await builder.ExecuteAsync(ct).ConfigureAwait(false);

			return result.HasRows;
		}
	}
}
