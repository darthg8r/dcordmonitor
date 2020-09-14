using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord
{
	public class DiscordOptions 
	{
		public DiscordOptions()
		{
			ChannelMappings = new List<ChannelMapping>();
		}

		public string DiscordToken { get; set; }

		public List<ChannelMapping> ChannelMappings { get; set; }
	}

	public class ChannelMapping
	{
		public string Name { get; set; }

		public ulong ServerId { get; set; }

		public ulong ChannelId { get; set; }

		public string Target { get; set; }
	}

	public class ServiceBusOptions
	{
		public string Persistence { get; set; }

		public string Transport { get; set; }
	}
}
