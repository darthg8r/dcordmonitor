using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.Web;
using NServiceBus;
using NServiceBus.Extensions.Logging;

namespace Discord
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseNServiceBus(hostBuilderContext =>
				{
					var config = new ConfigurationBuilder()
						.SetBasePath(Directory.GetCurrentDirectory())
						.AddJsonFile("appsettings.development.json", optional: true)
						.AddJsonFile("appsettings.json", optional: true)
						.Build();

					ServiceBusOptions serviceBusOptions = new ServiceBusOptions();
					config.GetSection("serviceBus").Bind(serviceBusOptions);



					var endpointConfiguration = new EndpointConfiguration("DarthBot.Discord");
					endpointConfiguration.EnableInstallers();
					var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();
					transport.ConnectionString(serviceBusOptions.Transport);

					var persistence = endpointConfiguration.UsePersistence<AzureStoragePersistence>();
					persistence.ConnectionString(serviceBusOptions.Persistence);

					//endpointConfiguration.SendOnly();
					endpointConfiguration.EnableUniformSession();
					return endpointConfiguration;
				})
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
					webBuilder.ConfigureLogging((hostingContext, logging) =>
						{
							logging.ClearProviders();
							logging.SetMinimumLevel(LogLevel.Trace);
						})
						.UseNLog();

					Microsoft.Extensions.Logging.ILoggerFactory extensionsLoggerFactory = new NLogLoggerFactory();

					NServiceBus.Logging.ILoggerFactory nservicebusLoggerFactory = new ExtensionsLoggerFactory(loggerFactory: extensionsLoggerFactory);

					NServiceBus.Logging.LogManager.UseFactory(loggerFactory: nservicebusLoggerFactory);
				});
	}
}
