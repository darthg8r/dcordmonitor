using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Discord
{
	public class DiscordMonitoringService : IHostedService
	{
		private readonly IOptions<DiscordOptions> _options;
		private readonly ILogger<DiscordMonitoringService> _logger;

		public DiscordMonitoringService(IOptions<DiscordOptions> options, ILogger<DiscordMonitoringService> logger)
		{
			_options = options;
			_logger = logger;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			var token = _options.Value.DiscordToken;

			var client = new DiscordSocketClient(new DiscordSocketConfig { LogLevel = LogSeverity.Debug });

			client.MessageReceived += Client_MessageReceived;


			await client.LoginAsync(TokenType.User, token);
			await client.StartAsync();

			client.Disconnected += async (Exception arg) =>
			{
				await client.LoginAsync(TokenType.User, token);
				await client.StartAsync();
			};
		}




		private async Task Client_MessageReceived(SocketMessage arg)
		{
			if (arg.Channel is SocketTextChannel textChannel)
			{
				HttpClient client = new HttpClient();

				foreach (var mapping in _options.Value.ChannelMappings)
				{
					if (mapping.ServerId == textChannel.Guild.Id && textChannel.Id == mapping.ChannelId)
					{

						var message =
							$"Author: {arg.Author}\n {arg.Content} {arg.Attachments.FirstOrDefault()?.Url}";

						await client.PostAsJsonAsync(mapping.Target,
							new
							{
								content = message
							});

					}
				}
			}


		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}