using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using SapphireDb.Command;

namespace SapphireDb.Models
{
    public class SapphireDatabaseOptions
    {
        public SapphireDatabaseOptions(bool strict = false)
        {
            if (strict)
            {
                CanExecuteCommand = (command, context) => context.User.Identity.IsAuthenticated;
                IsAllowedToSendMessages = (context) => context.User.Identity.IsAuthenticated;
                IsAllowedForTopicSubscribe = (context, topic) => context.User.Identity.IsAuthenticated;
                IsAllowedForTopicPublish = (context, topic) => context.User.Identity.IsAuthenticated;
            }
            else
            {
                CanExecuteCommand = (command, context) => true;
                IsAllowedToSendMessages = (context) => true;
                IsAllowedForTopicSubscribe = (context, topic) => true;
                IsAllowedForTopicPublish = (context, topic) => true;
            }
        }

        public SapphireDatabaseOptions(IConfigurationSection configuration, bool strict = false) : this(strict)
        {
            Sync = new SyncConfiguration(configuration.GetSection(nameof(Sync)));
            ApiConfigurations = configuration.GetSection(nameof(ApiConfigurations)).GetChildren().Select((section) => new ApiConfiguration(section)).ToList();
            ServerSentEventsInterface = configuration[nameof(ServerSentEventsInterface)]?.ToLowerInvariant() != "false";
            WebsocketInterface = configuration[nameof(WebsocketInterface)]?.ToLowerInvariant() != "false";
            PollInterface = configuration[nameof(PollInterface)]?.ToLowerInvariant() != "false";
            RestInterface = configuration[nameof(PollInterface)]?.ToLowerInvariant() != "false";
        }

        public List<ApiConfiguration> ApiConfigurations { get; set; } = new List<ApiConfiguration>();

        public Func<CommandBase, HttpInformation, bool> CanExecuteCommand { get; set; }

        public Func<HttpInformation, bool> IsAllowedToSendMessages { get; set; }

        public Func<HttpInformation, string, bool> IsAllowedForTopicSubscribe { get; set; }

        public Func<HttpInformation, string, bool> IsAllowedForTopicPublish { get; set; }

        public bool ServerSentEventsInterface { get; set; } = true;

        public bool WebsocketInterface { get; set; } = true;

        public bool PollInterface { get; set; } = true;

        public bool RestInterface { get; set; } = true;

        public SyncConfiguration Sync { get; set; } = new SyncConfiguration();

        public class SyncConfiguration
        {
            public SyncConfiguration()
            {

            }

            public SyncConfiguration(IConfigurationSection configurationSection)
            {
                Id = configurationSection[nameof(Id)];
                Secret = configurationSection[nameof(Secret)];
                Entries = configurationSection.GetSection(nameof(Entries)).GetChildren().Select((section) => new SyncEntry(section)).ToList();
            }

            public string Id { get; set; }

            public string Secret { get; set; }

            public List<SyncEntry> Entries { get; set; } = new List<SyncEntry>();
        }

        public class SyncEntry
        {
            public SyncEntry()
            {

            }

            public SyncEntry(IConfigurationSection configurationSection)
            {
                Url = configurationSection[nameof(Url)];
                Secret = configurationSection[nameof(Secret)];
                Id = configurationSection[nameof(Id)];
            }

            public string Url { get; set; }

            public string Secret { get; set; }

            public string Id { get; set; }
        }

        public class ApiConfiguration
        {
            public ApiConfiguration()
            {

            }

            public ApiConfiguration(IConfigurationSection configurationSection)
            {
                Key = configurationSection[nameof(Key)];
                Secret = configurationSection[nameof(Secret)];
                Name = configurationSection[nameof(Name)];
            }

            public string Key { get; set; }

            public string Secret { get; set; }

            public string Name { get; set; }
        }
    }
}
