using Microsoft.Extensions.Configuration;
using Serilog;

namespace SensateIoT.Platform.Network.Router.Application
{
	internal static class LoggingUtility
	{
		internal static Serilog.Core.Logger BuildLogger(IConfigurationRoot config, string env)
		{
			return new LoggerConfiguration()
				.ReadFrom.Configuration(config)
				.Enrich.FromLogContext()
				.Enrich.WithProperty("ApplicationName", typeof(Program).Assembly.GetName().Name)
				.Enrich.WithProperty("Environment", env)
				.CreateLogger();
		}
	}
}