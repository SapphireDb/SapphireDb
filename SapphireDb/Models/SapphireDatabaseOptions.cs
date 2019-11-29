using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SapphireDb.Models.Auth;
using Microsoft.Extensions.Configuration;
using SapphireDb.Command;
using SapphireDb.Command.Login;
using SapphireDb.Command.Renew;

namespace SapphireDb.Models
{
    public class SapphireDatabaseOptions
    {
        public SapphireDatabaseOptions()
        {
            CanExecuteCommand = (command, context) =>
                command is LoginCommand || command is RenewCommand || context.User.Identity.IsAuthenticated;
            AuthInfoAllowFunction = (context) => context.User.IsInRole("admin");
            AuthAllowFunction = (context) => context.User.IsInRole("admin");
            IsAllowedToSendMessages = (context) => context.User.Identity.IsAuthenticated;
            IsAllowedForTopicSubscribe = (context, topic) => context.User.Identity.IsAuthenticated;
            IsAllowedForTopicPublish = (context, topic) => context.User.Identity.IsAuthenticated;
            IsAllowedForConnectionManagement = (context) => context.User.IsInRole("admin");

        }

        public SapphireDatabaseOptions(IConfigurationSection configuration) : this()
        {
            Nlb = new NlbConfiguration(configuration.GetSection(nameof(Nlb)));
            ApiConfigurations = configuration.GetSection(nameof(ApiConfigurations)).GetChildren().Select((section) => new ApiConfiguration(section)).ToList();
            EnableAuthCommands = configuration[nameof(EnableAuthCommands)]?.ToLowerInvariant() != "false";
            ServerSentEventsInterface = configuration[nameof(ServerSentEventsInterface)]?.ToLowerInvariant() != "false";
            WebsocketInterface = configuration[nameof(WebsocketInterface)]?.ToLowerInvariant() != "false";
            PollInterface = configuration[nameof(PollInterface)]?.ToLowerInvariant() != "false";
        }

        public List<ApiConfiguration> ApiConfigurations { get; set; } = new List<ApiConfiguration>();

        internal bool EnableBuiltinAuth { get; set; }

        public bool EnableAuthCommands { get; set; } = true;


        public Func<CommandBase, HttpInformation, bool> CanExecuteCommand { get; set; }

        public Func<HttpInformation, bool> AuthInfoAllowFunction { get; set; }

        public Func<HttpInformation, bool> AuthAllowFunction { get; set; }

        public Func<HttpInformation, bool> IsAllowedToSendMessages { get; set; }

        public Func<HttpInformation, string, bool> IsAllowedForTopicSubscribe { get; set; }

        public Func<HttpInformation, string, bool> IsAllowedForTopicPublish { get; set; }

        public Func<HttpInformation, bool> IsAllowedForConnectionManagement { get; set; }


        public bool ServerSentEventsInterface { get; set; } = true;

        public bool WebsocketInterface { get; set; } = true;

        public bool PollInterface { get; set; } = true;

        public NlbConfiguration Nlb { get; set; } = new NlbConfiguration();

        public class NlbConfiguration
        {
            public NlbConfiguration()
            {

            }

            public NlbConfiguration(IConfigurationSection configurationSection)
            {
                Enabled = configurationSection.GetValue<bool>(nameof(Enabled));
                Id = configurationSection[nameof(Id)];
                Secret = configurationSection[nameof(Secret)];
                EncryptionKey = configurationSection[nameof(EncryptionKey)];
                Entries = configurationSection.GetSection(nameof(Entries)).GetChildren().Select((section) => new NlbEntry(section)).ToList();
            }

            public bool Enabled { get; set; }

            public string Id { get; set; }

            public string Secret { get; set; }

            public string EncryptionKey { get; set; }

            public List<NlbEntry> Entries { get; set; } = new List<NlbEntry>();
        }

        public class NlbEntry
        {
            public NlbEntry()
            {

            }

            public NlbEntry(IConfigurationSection configurationSection)
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
