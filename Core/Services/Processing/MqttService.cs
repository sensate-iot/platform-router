/*
 * MQTT background service
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MQTTnet;

using SensateService.Helpers;
using SensateService.Middleware;
using SensateService.Services.Settings;

namespace SensateService.Services.Processing
{
	public class MqttService : AbstractMqttService 
	{
		private readonly IServiceProvider _provider;
		private readonly ConcurrentDictionary<string, Type> _handlers;
		private readonly MqttServiceOptions _options;

		public MqttService(IServiceProvider provider, IOptions<MqttServiceOptions> options, ILogger<MqttService> logger) :
			base(options.Value.Host, options.Value.Port, options.Value.Ssl, logger)
		{
			this._provider = provider;
			this._handlers = new ConcurrentDictionary<string, Type>();
			this._options = options.Value;
		}

		public override async Task ExecuteAsync(CancellationToken token)
		{
			await this.Connect(this._options.Username, this._options.Password).AwaitBackground();
		}

		public void MapTopicHandler<T>(string topic) where T : MqttHandler
		{
			this._handlers.TryAdd(topic, typeof(T));
		}

		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			await base.StopAsync(cancellationToken);
			await this.Disconnect().AwaitBackground();

			this._logger.LogInformation("MQTT client disconnected!");
		}

		protected override async Task OnConnectAsync()
		{
			TopicFilterBuilder tfb;
			List<TopicFilter> filters;

			filters = new List<TopicFilter>();

			foreach(var tuple in this._handlers) {
				tfb = new TopicFilterBuilder().WithAtMostOnceQoS();
				tfb.WithTopic(tuple.Key);
				filters.Add(tfb.Build());
			}

			this._logger.LogInformation("MQTT client connected!");
			await this.Client.SubscribeAsync(filters).AwaitBackground();
		}

		protected override async Task OnMessageAsync(string topic, string msg)
		{
			MqttHandler handler;

			using(var scope = this._provider.CreateScope()) {
				if(!this._handlers.TryGetValue(topic, out Type handlerType)) {
					this._handlers.TryGetValue(this._options.TopicShare + topic, out handlerType);
				}

				handler = scope.ServiceProvider.GetRequiredService(handlerType) as MqttHandler;

				if(handler == null)
					return;

				try {
					await handler.OnMessageAsync(topic, msg).AwaitBackground();
				} catch(Exception ex) {
					this._logger.LogWarning($"Unable to store measurement: {ex.Message}");
				}
			}
		}
	}
}
