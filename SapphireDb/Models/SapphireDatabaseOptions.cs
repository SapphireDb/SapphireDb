using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using SapphireDb.Command;
using SapphireDb.Connection;

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
                DisableIncludePrefilter = true;
                DisableSelectPrefilter = true;
            }
            else
            {
                CanExecuteCommand = (command, context) => true;
                IsAllowedToSendMessages = (context) => true;
            }
        }

        public SapphireDatabaseOptions(IConfigurationSection configuration, bool strict = false) : this(strict)
        {
            // TODO: add automatic parsing
            ApiConfigurations = configuration.GetSection(nameof(ApiConfigurations)).GetChildren().Select((section) => new ApiConfiguration(section)).ToList();
            RestInterface = configuration[nameof(RestInterface)]?.ToLowerInvariant() != "false";
            DisableIncludePrefilter = configuration[nameof(DisableIncludePrefilter)]?.ToLowerInvariant() == "true";
            DisableSelectPrefilter = configuration[nameof(DisableSelectPrefilter)]?.ToLowerInvariant() == "true";
        }

        public List<ApiConfiguration> ApiConfigurations { get; set; } = new List<ApiConfiguration>();

        public Func<CommandBase, IConnectionInformation, bool> CanExecuteCommand { get; set; }

        public Func<IConnectionInformation, bool> IsAllowedToSendMessages { get; set; }

        public bool RestInterface { get; set; } = true;

        public bool DisableIncludePrefilter { get; set; } = false;

        public bool DisableSelectPrefilter { get; set; } = false;

        public long MaximumReceiveMessageSize { get; set; } = Int64.MaxValue;

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
