/*
 * Application startup.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Twilio;
using Twilio.Rest.Api.V2010.Account;

using SensateIoT.Platform.Network.Adapters.Abstract;
using SensateIoT.Platform.Network.Adapters.Mail;
using SensateIoT.Platform.Network.Adapters.SMS;
using SensateIoT.Platform.Network.Common.Init;
using SensateIoT.Platform.Network.Common.Services.Metrics;
using SensateIoT.Platform.Network.Common.Settings;
using SensateIoT.Platform.Network.DataAccess.Abstract;
using SensateIoT.Platform.Network.DataAccess.Repositories;
using SensateIoT.Platform.Network.TriggerService.Clients;
using SensateIoT.Platform.Network.TriggerService.Config;
using SensateIoT.Platform.Network.TriggerService.MQTT;
using SensateIoT.Platform.Network.TriggerService.Services;
using SensateIoT.Platform.Network.TriggerService.Settings;

namespace SensateIoT.Platform.Network.TriggerService.Application
{

	public class Startup
	{
		private readonly IConfiguration Configuration;

		public Startup(IConfiguration configuration)
		{
			this.Configuration = configuration;
		}

		public void ConfigureServices(IServiceCollection services)
		{
			var db = new DatabaseConfig();
			var mqtt = new MqttConfig();
			var timeouts = new TimeoutConfig();
			var mail = new MailConfig();
			var text = new TextConfig();

			this.Configuration.GetSection("Mail").Bind(mail);
			this.Configuration.GetSection("Database").Bind(db);
			this.Configuration.GetSection("Mqtt").Bind(mqtt);
			this.Configuration.GetSection("Timeouts").Bind(timeouts);
			this.Configuration.GetSection("Text").Bind(text);

			var privatemqtt = mqtt.InternalBroker;

			services.AddConnectionStrings(db.Networking.ConnectionString, db.SensateIoT.ConnectionString);
			services.AddAuthorizationContext();
			services.AddNetworkingContext();
			services.AddDocumentStore(db.MongoDB.ConnectionString, db.MongoDB.DatabaseName, db.MongoDB.MaxConnections);
			services.Configure<TimeoutConfig>(this.Configuration.GetSection("Timeouts"));

			services.Configure<MetricsOptions>(this.Configuration.GetSection("HttpServer:Metrics"));
			services.AddHostedService<MetricsService>();

			services.AddInternalMqttService(options => {
				options.Ssl = privatemqtt.Ssl;
				options.Host = privatemqtt.Host;
				options.Port = privatemqtt.Port;
				options.Username = privatemqtt.Username;
				options.Password = privatemqtt.Password;
				options.Id = Guid.NewGuid().ToString();
				options.TopicShare = "$share/triggers/";
			});

			services.Configure<RouterSettings>(s => {
				s.Host = this.Configuration.GetValue<string>("Router:Host");
				s.Port = this.Configuration.GetValue<ushort>("Router:Port");
				s.Secure = this.Configuration.GetValue<bool>("Router:Secure");
			});

			if(mail.Provider == "SendGrid") {
				services.AddSingleton<IEmailSender, SendGridMailer>();
				services.Configure<SendGridAuthOptions>(opts => {
					opts.FromName = mail.FromName;
					opts.From = mail.From;
					opts.Key = mail.SendGrid.Key;
					opts.Username = mail.SendGrid.Username;
				});
			} else if(mail.Provider == "SMTP") {
				services.AddSingleton<IEmailSender, SmtpMailer>();
				services.Configure<SmtpAuthOptions>(opts => {
					opts.FromName = mail.FromName;
					opts.From = mail.From;
					opts.Password = mail.Smtp.Password;
					opts.Username = mail.Smtp.Username;
					opts.Ssl = mail.Smtp.Ssl;
					opts.Port = mail.Smtp.Port;
					opts.Host = mail.Smtp.Host;
				});
			}

			if(text.Provider == "Twillio") {
				TwilioClient.Init(text.Twilio.AccountSid, text.Twilio.AuthToken);

				var incoming = IncomingPhoneNumberResource.Fetch(pathSid: text.Twilio.PhoneSid);
				services.Configure<TextServiceSettings>(options => {
					options.AlphaCode = text.AlphaCode;
					options.PhoneNumber = incoming.PhoneNumber.ToString();
				});

				services.AddScoped<ITextSendService, TwillioTextSendService>();
			} else {
				Console.WriteLine("Text message provider not configured!");
			}

			services.AddScoped<IControlMessageRepository, ControlMessageRepository>();
			services.AddScoped<ITriggerRepository, TriggerRepository>();
			services.AddScoped<IRoutingRepository, RoutingRepository>();
			services.AddScoped<IControlMessageRepository, ControlMessageRepository>();
			services.AddScoped<ITriggerActionExecutionService, TriggerActionExecutionService>();
			services.AddSingleton<IDataPointMatchingService, DataPointMatchingService>();
			services.AddSingleton<IRegexMatchingService, RegexMatchingService>();
			services.AddSingleton<IRouterClient, RouterClient>();
			services.AddSingleton<IEmailSender, SmtpMailer>();

			services.AddHttpClient();
			services.AddMqttHandlers();
		}

		public void Configure(IServiceProvider provider)
		{
			var mqtt = new MqttConfig();

			this.Configuration.GetSection("Mqtt").Bind(mqtt);
			var @private = mqtt.InternalBroker;

			provider.MapInternalMqttTopic<MqttBulkNumberTriggerHandler>(@private.BulkMeasurementTopic);
			provider.MapInternalMqttTopic<MqttRegexTriggerHandler>(@private.BulkMessageTopic);
		}
	}
}
