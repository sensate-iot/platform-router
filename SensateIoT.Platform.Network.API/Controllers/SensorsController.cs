/*
 * Sensor HTTP controller.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using MongoDB.Bson;
using Npgsql;

using SensateIoT.Platform.Network.API.Abstract;
using SensateIoT.Platform.Network.API.Attributes;
using SensateIoT.Platform.Network.API.DTO;
using SensateIoT.Platform.Network.Common.Caching.Abstract;
using SensateIoT.Platform.Network.Data.DTO;
using SensateIoT.Platform.Network.Data.Enums;
using SensateIoT.Platform.Network.Data.Models;
using SensateIoT.Platform.Network.DataAccess.Abstract;

using ApiKey = SensateIoT.Platform.Network.Data.Models.ApiKey;
using Sensor = SensateIoT.Platform.Network.Data.Models.Sensor;

namespace SensateIoT.Platform.Network.API.Controllers
{
	[Produces("application/json")]
	[Route("network/v1/[controller]")]
	public class SensorsController : AbstractApiController
	{
		private readonly ILogger<SensorsController> m_logger;
		private readonly ISensorService m_sensorService;
		private readonly ICommandPublisher m_mqtt;
		private readonly IAccountRepository m_accounts;
		private readonly ITriggerRepository m_triggers;
		private readonly IDistributedCache<PaginationResponse<Sensor>> m_cache;
		private readonly Random m_rng;

		private const string Symbols = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@!_-";
		private const int SensorSecretLength = 32;

		public SensorsController(IHttpContextAccessor ctx,
								 ISensorRepository sensors,
								 ILogger<SensorsController> logger,
								 ISensorLinkRepository links,
								 ITriggerRepository triggers,
								 ISensorService sensorService,
								 IAccountRepository accounts,
								 IApiKeyRepository keys,
								 IDistributedCache<PaginationResponse<Sensor>> cache,
								 ICommandPublisher mqtt) : base(ctx, sensors, links, keys)
		{
			this.m_logger = logger;
			this.m_sensorService = sensorService;
			this.m_mqtt = mqtt;
			this.m_triggers = triggers;
			this.m_accounts = accounts;
			this.m_cache = cache;
			this.m_rng = new Random(DateTime.UtcNow.Millisecond * DateTime.UtcNow.Second);
		}

		private static string NextStringWithSymbols(Random rng, int length)
		{
			char[] ary;

			ary = Enumerable.Repeat(Symbols, length)
				.Select(s => s[rng.Next(0, Symbols.Length)]).ToArray();
			return new string(ary);
		}

		[HttpGet("{sensorId}/triggers")]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status422UnprocessableEntity)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(PaginationResponse<Trigger>), StatusCodes.Status200OK)]
		public async Task<IActionResult> Get(string sensorId, [FromQuery] TriggerType? type)
		{
			var invalidResponse = new Response<string>();
			var err = false;

			if(string.IsNullOrEmpty(sensorId)) {
				invalidResponse.AddError("Missing value in 'sensorId' attribute.");
				return this.UnprocessableEntity(invalidResponse);
			}

			if(!ObjectId.TryParse(sensorId, out _)) {
				invalidResponse.AddError("Invalid value in 'sensorId' attribute.");
				err = true;
			}

			if(type == null) {
				invalidResponse.AddError("Trigger type not specified.");
				err = true;
			}

			if(err) {
				return this.UnprocessableEntity(invalidResponse);
			}

			var linked = await this.IsLinkedSensor(sensorId).ConfigureAwait(false);
			var auth = await this.AuthenticateUserForSensor(sensorId).ConfigureAwait(false);

			if(!auth && !linked) {
				return this.CreateNotAuthorizedResult();
			}

			var triggers = await this.m_triggers.GetAsync(sensorId, type.Value).ConfigureAwait(false);
			var listed = triggers.ToList();
			var paginationResult = new PaginationResponse<Trigger> {
				Data = new PaginationResult<Trigger> {
					Values = listed,
					Count = listed.Count,
				}
			};

			return this.Ok(paginationResult);
		}

		[HttpGet]
		[ActionName("FindSensorsByName")]
		[ProducesResponseType(typeof(PaginationResponse<Sensor>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status422UnprocessableEntity)]
		public async Task<IActionResult> Index([FromQuery] string name, [FromQuery] int skip = 0, [FromQuery] int limit = 0, [FromQuery] bool link = true)
		{
			PaginationResponse<Sensor> sensors;
			var sw = Stopwatch.StartNew();
			var key = this.GenerateCacheKey(name, skip, limit, link);

			try {
				sensors = await this.m_cache.GetAsync(key).ConfigureAwait(false);
				return this.Ok(sensors);
			} catch(ArgumentOutOfRangeException) {
				this.m_logger.LogDebug("Unable to find cache key: {key} during sensor lookup.", key);
			}

			if(!link) {
				if(string.IsNullOrEmpty(name)) {
					sensors = await this.GetSensors(skip, limit).ConfigureAwait(false);
				} else {
					sensors = await this.GetSensors(name, skip, limit).ConfigureAwait(false);
				}
			} else {
				sensors = await this.GetSensorsWithLinks(name, skip, limit).ConfigureAwait(false);
			}

			sw.Stop();
			this.m_logger.LogInformation($"Fetching sensors took: {sw.ElapsedMilliseconds}ms");
			await this.m_cache.SetAsync(key, sensors, new CacheEntryOptions { Timeout = TimeSpan.FromMinutes(5) }).ConfigureAwait(false);

			return this.Ok(sensors);
		}

		private async Task<PaginationResponse<Sensor>> GetSensors(string name, int skip, int limit)
		{
			PaginationResponse<Sensor> sensors;

			var s = await this.m_sensors.FindByNameAsync(this.CurrentUser.ID, name, skip, limit).ConfigureAwait(false);
			var count = await this.m_sensors.CountAsync(this.CurrentUser, name).ConfigureAwait(false);

			sensors = new PaginationResponse<Sensor> {
				Data = new PaginationResult<Sensor> {
					Count = (int)count,
					Skip = skip,
					Limit = limit,
					Values = s
				}
			};

			return sensors;
		}

		private async Task<PaginationResponse<Sensor>> GetSensors(int skip, int limit)
		{
			PaginationResponse<Sensor> sensors;

			var s = await this.m_sensors.GetAsync(this.CurrentUser.ID, skip, limit).ConfigureAwait(false);
			var count = await this.m_sensors.CountAsync(this.CurrentUser).ConfigureAwait(false);

			sensors = new PaginationResponse<Sensor> {
				Data = new PaginationResult<Sensor> {
					Count = (int)count,
					Skip = skip,
					Limit = limit,
					Values = s
				}
			};

			return sensors;
		}

		private async Task<PaginationResponse<Sensor>> GetSensorsWithLinks(string name, int skip, int limit)
		{
			PaginationResponse<Sensor> sensors;

			if(string.IsNullOrEmpty(name)) {
				sensors = new PaginationResponse<Sensor> {
					Data = await this.m_sensorService.GetSensorsAsync(this.CurrentUser, skip, limit)
						.ConfigureAwait(false)
				};
			} else {
				sensors = new PaginationResponse<Sensor> {
					Data = await this.m_sensorService.GetSensorsAsync(this.CurrentUser, name, skip, limit)
						.ConfigureAwait(false)
				};
			}

			sensors.Data.Skip = skip;
			sensors.Data.Limit = limit;

			return sensors;
		}

		[HttpPost]
		[ReadWriteApiKey, ValidateModel]
		[ProducesResponseType(typeof(Response<Sensor>), StatusCodes.Status201Created)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status422UnprocessableEntity)]
		public async Task<IActionResult> Post([FromBody] Sensor sensor)
		{
			sensor.Owner = this.m_currentUserId;

			if(string.IsNullOrWhiteSpace(sensor.Secret)) {
				sensor.Secret = NextStringWithSymbols(this.m_rng, SensorSecretLength);
			}

			var cache = this.m_cache.RemoveAsync(this.GenerateCacheKey(null, 0, 10, true));
			await this.m_keys.CreateSensorKeyAsync(sensor).ConfigureAwait(false);
			await this.m_sensors.CreateAsync(sensor).ConfigureAwait(false);

			await Task.WhenAll(
				this.m_mqtt.PublishCommandAsync(CommandType.AddSensor, sensor.InternalId.ToString()),
				this.m_mqtt.PublishCommandAsync(CommandType.AddKey, sensor.Secret)
			).ConfigureAwait(false);

			await cache.ConfigureAwait(false);

			return this.CreatedAtAction(nameof(this.Get), new { Id = sensor.InternalId }, new Response<Sensor>(sensor));
		}

		[HttpDelete("links")]
		[ReadWriteApiKey, ValidateModel]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status422UnprocessableEntity)]
		public async Task<IActionResult> DeleteLink([FromBody] SensorLink link)
		{
			var myemail = this.CurrentUser.Email;

			try {
				var auth = await this.AuthenticateUserForSensor(link.SensorId).ConfigureAwait(false);
				auth |= myemail == link.UserId;

				var user = await this.m_accounts.GetAccountByEmailAsync(link.UserId).ConfigureAwait(false);

				if(user == null) {
					var response = new Response<string>();

					response.AddError("Invalid 'userId'.");
					return this.UnprocessableEntity(response);
				}

				if(!auth) {
					return this.Forbidden();
				}

				link.UserId = user.ID.ToString();
				await this.m_links.DeleteAsync(link).ConfigureAwait(false);
			} catch(FormatException ex) {
				var response = new Response<string>();

				response.AddError(ex.Message);
				return this.UnprocessableEntity(response);
			}

			return this.NoContent();
		}

		[HttpGet("{sensorId}/links")]
		[ProducesResponseType(typeof(PaginationResponse<SensorLink>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status422UnprocessableEntity)]
		public async Task<IActionResult> GetLinks([NotNull] string sensorId)
		{
			IEnumerable<SensorLink> links;
			PaginationResponse<SensorLink> response;

			var sensorTask = this.m_sensors.GetAsync(sensorId);

			links = await this.m_links.GetAsync(sensorId);

			var sensor = await sensorTask.ConfigureAwait(false);

			if(sensor == null) {
				return this.NotFound();
			}

			var linkList = links.ToList();
			var users = await this.m_accounts.GetAccountsAsync(linkList.Select(x => x.UserId)).ConfigureAwait(false);
			var dict = users.ToDictionary(x => x.ID, x => x);

			if(!string.IsNullOrEmpty(sensorId) && this.m_currentUserId != sensor.Owner) {
				linkList.RemoveAll(x => x.UserId != this.m_currentUserId);
			}

			foreach(var link in linkList) {
				if(!dict.TryGetValue(Guid.Parse(link.UserId), out var user)) {
					continue;
				}

				link.UserId = user.Email;
			}

			response = new PaginationResponse<SensorLink> {
				Data = new PaginationResult<SensorLink> {
					Values = linkList,
					Count = linkList.Count,
					Limit = 0,
					Skip = 0
				}
			};

			return this.Ok(response);
		}

		[HttpPost("links")]
		[ReadWriteApiKey, ValidateModel]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status422UnprocessableEntity)]
		public async Task<IActionResult> Link([FromBody] SensorLink link)
		{
			var status = new Response<string>();
			var error = false;

			try {

				if(link.UserId == this.CurrentUser.Email) {
					status.AddError("Unable to link sensor to own account!");
					return this.BadRequest(status);
				}

				var sensor = await this.m_sensors.GetAsync(link.SensorId).ConfigureAwait(false);
				var user = await this.m_accounts.GetAccountByEmailAsync(link.UserId).ConfigureAwait(false);

				if(!await this.AuthenticateUserForSensor(sensor, false).ConfigureAwait(false)) {
					return this.Forbidden();
				}

				if(user == null) {
					status.AddError("Target user not found.");
					error = true;
				}

				if(sensor == null) {
					status.AddError("Target sensor not found.");
					error = true;
				}

				if(error) {
					return this.BadRequest(status);
				}

				link.UserId = user.ID.ToString();

				await this.m_links.CreateAsync(link).ConfigureAwait(false);

			} catch(PostgresException ex) {
				if(ex.SqlState != PostgresErrorCodes.UniqueViolation) {
					return this.StatusCode(500);
				}

				var response = new Response<string>();

				response.AddError("This link already exists!");
				return this.UnprocessableEntity(response);
			} catch(FormatException ex) {
				var response = new Response<string>();

				response.AddError(ex.Message);
				return this.UnprocessableEntity(response);
			}

			return this.NoContent();
		}

		[HttpPut("{id}")]
		[ReadWriteApiKey, ValidateModel]
		[ProducesResponseType(typeof(Response<Sensor>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status422UnprocessableEntity)]
		public async Task<IActionResult> Put(string id, [FromBody] SensorUpdate update)
		{
			Sensor sensor;

			if(!ObjectId.TryParse(id, out _)) {
				var response = new Response<string>();

				response.AddError("Unable to parse sensor ID");
				return this.UnprocessableEntity(response);
			}

			sensor = await this.m_sensors.GetAsync(id).ConfigureAwait(false);

			if(!await this.AuthenticateUserForSensor(sensor, false).ConfigureAwait(false)) {
				return this.Forbidden();
			}

			if(sensor == null) {
				return this.NotFound();
			}

			ApplyUpdate(sensor, update);
			await this.m_mqtt.PublishCommandAsync(CommandType.FlushSensor, sensor.InternalId.ToString()).ConfigureAwait(false);

			var cache = this.m_cache.RemoveAsync(this.GenerateCacheKey(null, 0, 10, true));
			var sensordb = this.m_sensors.UpdateAsync(sensor);
			await Task.WhenAll(cache, sensordb).ConfigureAwait(false);
			await this.m_mqtt.PublishCommandAsync(CommandType.AddSensor, sensor.InternalId.ToString()).ConfigureAwait(false);

			return this.Ok(new Response<Sensor>(sensor));
		}

		private static void ApplyUpdate(Sensor sensor, SensorUpdate update)
		{
			if(!string.IsNullOrEmpty(update.Name)) {
				sensor.Name = update.Name;
			}

			if(!string.IsNullOrEmpty(update.Description)) {
				sensor.Description = update.Description;
			}

			if(update.StorageEnabled.HasValue) {
				sensor.StorageEnabled = update.StorageEnabled.Value;
			}
		}

		private async Task<bool> UpdateSecret(Sensor sensor, SensorUpdate update)
		{
			ApiKey key = null;

			if(!string.IsNullOrEmpty(update.Secret)) {
				key = await this.m_keys.GetAsync(update.Secret).ConfigureAwait(false);
			}

			if(key != null) {
				return false; /* Key already exists */
			}

			var old = await this.m_keys.GetAsync(sensor.Secret).ConfigureAwait(false);

			var oldSecret = sensor.Secret;
			sensor.Secret = string.IsNullOrEmpty(update.Secret) ? NextStringWithSymbols(this.m_rng, SensorSecretLength) : update.Secret;

			await Task.WhenAll(
				this.m_mqtt.PublishCommandAsync(CommandType.FlushSensor, sensor.InternalId.ToString()),
				this.m_mqtt.PublishCommandAsync(CommandType.FlushKey, oldSecret)
			).ConfigureAwait(false);

			var cache = this.m_cache.RemoveAsync(this.GenerateCacheKey(null, 0, 10, true));
			await Task.WhenAll(
				this.m_keys.UpdateAsync(old.Key, sensor.Secret),
				this.m_sensors.UpdateSecretAsync(sensor.InternalId, sensor.Secret),
				cache
			).ConfigureAwait(false);

			return true;
		}

		[HttpPut("{id}/secret")]
		[ReadWriteApiKey]
		[ProducesResponseType(typeof(Response<Sensor>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status422UnprocessableEntity)]
		public async Task<IActionResult> PutSecret(string id, [FromBody] SensorUpdate update)
		{
			Sensor sensor;

			update ??= new SensorUpdate();

			if(!ObjectId.TryParse(id, out _)) {
				var response = new Response<string>();

				response.AddError("Unable to parse sensor ID");
				return this.UnprocessableEntity(response);
			}

			sensor = await this.m_sensors.GetAsync(id).ConfigureAwait(false);

			if(!await this.AuthenticateUserForSensor(sensor, false).ConfigureAwait(false)) {
				return this.Forbidden();
			}

			if(sensor == null) {
				return this.NotFound();
			}

			var success = await this.UpdateSecret(sensor, update).ConfigureAwait(false);

			if(!success) {
				var resp = new Response<string>();

				resp.AddError("Unable to update sensor secret.");
				return this.BadRequest(resp);
			}

			ApplyUpdate(sensor, update);
			await this.m_sensors.UpdateAsync(sensor).ConfigureAwait(false); /* Update other settings */

			await Task.WhenAll(this.m_mqtt.PublishCommandAsync(CommandType.AddKey, sensor.Secret),
							   this.m_mqtt.PublishCommandAsync(CommandType.AddSensor, sensor.InternalId.ToString())
			).ConfigureAwait(false);

			return this.Ok(new Response<Sensor>(sensor));
		}

		[HttpDelete("{id}")]
		[ReadWriteApiKey]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status422UnprocessableEntity)]
		public async Task<IActionResult> Delete(string id)
		{
			try {
				var sensor = await this.m_sensors.GetAsync(id).ConfigureAwait(false);

				if(sensor == null) {
					return this.NotFound();
				}

				if(!await this.AuthenticateUserForSensor(sensor, false).ConfigureAwait(false)) {
					return this.Forbidden();
				}

				var cache = this.m_cache.RemoveAsync(this.GenerateCacheKey(null, 0, 10, true));
				await this.m_sensorService.DeleteAsync(sensor, CancellationToken.None).ConfigureAwait(false);

				await Task.WhenAll(
					cache,
					this.m_mqtt.PublishCommandAsync(CommandType.FlushSensor, sensor.InternalId.ToString()),
					this.m_mqtt.PublishCommandAsync(CommandType.FlushKey, sensor.Secret)
				).ConfigureAwait(false);
			} catch(FormatException ex) {
				this.m_logger.LogInformation($"Unable to remove sensor: {ex.Message}");
				this.m_logger.LogDebug(ex.StackTrace);

				var response = new Response<string>();

				response.AddError(ex.Message);
				return this.UnprocessableEntity(response);
			}

			return this.NoContent();
		}

		[HttpGet("{id}")]
		[ProducesResponseType(typeof(Response<Sensor>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status422UnprocessableEntity)]
		public async Task<IActionResult> Get(string id)
		{
			Sensor sensor;

			try {
				sensor = await this.m_sensors.GetAsync(id).ConfigureAwait(false);
			} catch(FormatException ex) {
				var response = new Response<string>();

				response.AddError(ex.Message);
				return this.UnprocessableEntity(response);
			}

			if(sensor == null) {
				return this.NotFound();
			}

			var linked = await this.IsLinkedSensor(id).ConfigureAwait(false);

			if(!await this.AuthenticateUserForSensor(sensor, false).ConfigureAwait(false) && !linked) {
				return this.Forbidden();
			}

			return this.Ok(new Response<Sensor> { Data = sensor });
		}

		private IActionResult CreateNotAuthorizedResult()
		{
			var response = new Response<string>();

			response.AddError("Unable to authorize user!");
			return this.Unauthorized(response);
		}

		private string GenerateCacheKey(string name, int skip, int limit, bool link)
		{
			var nameForKey = string.IsNullOrEmpty(name) ? "default" : name;
			return $"{this.m_currentUserId}::{nameForKey}::{skip}::{limit}::{link}";
		}
	}
}
