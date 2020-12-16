/*
 * Sensor HTTP controller.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using SensateIoT.Platform.Network.API.Abstract;
using SensateIoT.Platform.Network.API.Attributes;
using SensateIoT.Platform.Network.API.DTO;
using SensateIoT.Platform.Network.Data.Enums;
using SensateIoT.Platform.Network.Data.Models;
using SensateIoT.Platform.Network.DataAccess.Abstract;

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
		private readonly Random m_rng;

		private const string Symbols = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@!_-";
		private const int SensorSecretLength = 32;

		public SensorsController(IHttpContextAccessor ctx,
								 ISensorRepository sensors,
								 ILogger<SensorsController> logger,
								 ISensorLinkRepository links,
								 ISensorService sensorService,
								 IAccountRepository accounts,
								 IApiKeyRepository keys,
								 ICommandPublisher mqtt) : base(ctx, sensors, links, keys)
		{
			this.m_logger = logger;
			this.m_sensorService = sensorService;
			this.m_mqtt = mqtt;
			this.m_accounts = accounts;
			this.m_rng = new Random(DateTime.UtcNow.Millisecond * DateTime.UtcNow.Second);
		}

		private static string NextStringWithSymbols(Random rng, int length)
		{
			char[] ary;

			ary = Enumerable.Repeat(Symbols, length)
				.Select(s => s[rng.Next(0, Symbols.Length)]).ToArray();
			return new string(ary);
		}

		[HttpGet]
		[ActionName("FindSensorsByName")]
		[ProducesResponseType(typeof(PaginationResponse<Sensor>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status422UnprocessableEntity)]
		public async Task<IActionResult> Index([FromQuery] string name, [FromQuery] int skip = 0, [FromQuery] int limit = 0, [FromQuery] bool link = true)
		{
			PaginationResponse<Sensor> sensors;

			if(!link) {
				if(string.IsNullOrEmpty(name)) {
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
				} else {
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
				}
			} else {
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
			}


			return this.Ok(sensors);
		}

		[HttpPost]
		[ReadWriteApiKey, ValidateModel]
		[ProducesResponseType(typeof(Sensor), StatusCodes.Status201Created)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status422UnprocessableEntity)]
		public async Task<IActionResult> Post([FromBody] Sensor sensor)
		{
			sensor.Owner = this.m_currentUserId;

			if(string.IsNullOrWhiteSpace(sensor.Secret)) {
				sensor.Secret = NextStringWithSymbols(this.m_rng, SensorSecretLength);
			}

			await this.m_keys.CreateSensorKeyAsync(sensor).ConfigureAwait(false);
			await this.m_sensors.CreateAsync(sensor).ConfigureAwait(false);
			await this.m_mqtt.PublishCommandAsync(CommandType.AddSensor, sensor.InternalId.ToString()).ConfigureAwait(false);

			return this.CreatedAtAction(nameof(this.Get), new { Id = sensor.InternalId }, sensor);
		}

		[HttpDelete("links")]
		[ReadWriteApiKey, ValidateModel]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status422UnprocessableEntity)]
		public async Task<IActionResult> DeleteLink([FromBody] SensorLink link)
		{
			var myemail = this.CurrentUser.Email;

			var auth = await this.AuthenticateUserForSensor(link.SensorId).ConfigureAwait(false);
			auth |= myemail == link.UserId;

			if(!auth) {
				return this.Forbid();
			}

			link.UserId = this.m_currentUserId;
			await this.m_links.DeleteAsync(link).ConfigureAwait(false);

			return this.NoContent();
		}

		[HttpGet("links")]
		[ProducesResponseType(typeof(IEnumerable<SensorLink>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status422UnprocessableEntity)]
		public async Task<IActionResult> GetLinks([FromQuery][NotNull] string sensorId)
		{
			IEnumerable<SensorLink> links;

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

			return this.Ok(linkList);
		}

		[HttpPost("links")]
		[ReadWriteApiKey, ValidateModel]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status422UnprocessableEntity)]
		public async Task<IActionResult> Link([FromBody] SensorLink link)
		{
			var status = new Response<string>();
			var error = false;

			if(link.UserId == this.CurrentUser.Email) {
				status.AddError("Unable to link sensor to own account!");
				return this.BadRequest(status);
			}

			var sensor = await this.m_sensors.GetAsync(link.SensorId).ConfigureAwait(false);
			var user = await this.m_accounts.GetAccountByEmailAsync(link.UserId).ConfigureAwait(false);

			if(!await this.AuthenticateUserForSensor(sensor, false).ConfigureAwait(false)) {
				return this.Forbid();
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

			return this.NoContent();
		}

		[HttpPut("{id}")]
		[ReadWriteApiKey, ValidateModel]
		[ProducesResponseType(typeof(Response<Sensor>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status403Forbidden)]
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
				return this.Forbid();
			}

			if(sensor == null) {
				return this.NotFound();
			}

			if(!string.IsNullOrEmpty(update.Name)) {
				sensor.Name = update.Name;
			}

			if(!string.IsNullOrEmpty(update.Description)) {
				sensor.Description = update.Description;
			}

			await this.m_sensors.UpdateAsync(sensor).ConfigureAwait(false);

			return this.Ok(new Response<Sensor>(sensor));
		}

		[HttpPatch("{id}/secret")]
		[ReadWriteApiKey]
		[ProducesResponseType(typeof(Response<Sensor>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status422UnprocessableEntity)]
		public async Task<IActionResult> PatchSecret(string id, [FromBody] SensorUpdate update)
		{
			Sensor sensor;

			update ??= new SensorUpdate();
			if(!ObjectId.TryParse(id, out _)) {
				var response = new Response<string>();

				response.AddError("Unable to parse sensor ID");
				return this.UnprocessableEntity(response);
			}

			try {
				sensor = await this.m_sensors.GetAsync(id).ConfigureAwait(false);

				if(!await this.AuthenticateUserForSensor(sensor, false).ConfigureAwait(false)) {
					return this.Forbid();
				}

				if(sensor == null) {
					return this.NotFound();
				}

				var key = await this.m_keys.GetAsync(update.Secret).ConfigureAwait(false);
				var old = await this.m_keys.GetAsync(sensor.Secret).ConfigureAwait(false);

				if(key != null) {
					var resp = new Response<string>();

					resp.AddError("Unable to update sensor secret.");
					return this.BadRequest(resp);
				}

				var oldSecret = sensor.Secret;
				sensor.Secret = string.IsNullOrEmpty(update.Secret) ? NextStringWithSymbols(this.m_rng, SensorSecretLength) : update.Secret;

				if(!string.IsNullOrEmpty(update.Name)) {
					sensor.Name = update.Name;
				}

				if(!string.IsNullOrEmpty(update.Description)) {
					sensor.Description = update.Description;
				}

				await Task.WhenAll(
					this.m_keys.RefreshAsync(old, sensor.Secret),
					this.m_sensors.UpdateSecretAsync(sensor, old)
					).ConfigureAwait(false);
				await this.m_sensors.UpdateAsync(sensor).ConfigureAwait(false);

				await Task.WhenAll(
					this.m_mqtt.PublishCommandAsync(CommandType.FlushSensor, sensor.InternalId.ToString()),
					this.m_mqtt.PublishCommandAsync(CommandType.FlushKey, oldSecret)
				).ConfigureAwait(false);

				await Task.WhenAll(this.m_publish.PublishCommand(AuthServiceCommand.AddKey, sensor.Secret),
								   this.m_publish.PublishCommand(AuthServiceCommand.AddSensor,
																 sensor.InternalId.ToString())
				).ConfigureAwait(false);
			} catch(Exception ex) {
				this.m_logger.LogInformation($"Unable to update sensor: {ex.Message}");

				return this.BadRequest(new Status {
					Message = "Unable to update sensor.",
					ErrorCode = ReplyCode.BadInput
				});
			}

			return this.Ok(sensor);
		}

		/*[HttpDelete("{id}")]
		[ReadWriteApiKey]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status400BadRequest)]
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
					return this.Forbid();
				}

				await this.m_sensorService.DeleteAsync(sensor, CancellationToken.None).ConfigureAwait(false);
				await Task.WhenAll(
					this.m_publish.PublishCommand(AuthServiceCommand.FlushSensor, sensor.InternalId.ToString()),
					this.m_publish.PublishCommand(AuthServiceCommand.FlushKey, sensor.Secret)
				).ConfigureAwait(false);
			} catch(Exception ex) {
				this.m_logger.LogInformation($"Unable to remove sensor: {ex.Message}");
				this.m_logger.LogDebug(ex.StackTrace);

				return this.BadRequest(new Status {
					Message = "Unable to delete sensor.",
					ErrorCode = ReplyCode.UnknownError
				});
			}

			return this.NoContent();
		}*/

		[HttpGet("{id}")]
		[ProducesResponseType(typeof(Sensor), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(Response<string>), StatusCodes.Status400BadRequest)]
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
				return this.Forbid();
			}

			return this.Ok(new Response<Sensor> { Data = sensor });
		}
	}
}
