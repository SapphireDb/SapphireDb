using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace SapphireDb.Sync.Http
{
    public class HttpSyncConfiguration
    {
        public HttpSyncConfiguration()
        {

        }

        public HttpSyncConfiguration(IConfigurationSection configurationSection)
        {
            Id = configurationSection[nameof(Id)];
            Secret = configurationSection[nameof(Secret)];
            Entries = configurationSection.GetSection(nameof(Entries)).GetChildren().Select((section) => new HttpSyncEntry(section)).ToList();
        }
        
        public string Id { get; set; }

        public string Secret { get; set; }
        
        public List<HttpSyncEntry> Entries { get; set; } = new List<HttpSyncEntry>();
    }
    
    public class HttpSyncEntry
    {
        public HttpSyncEntry()
        {

        }

        public HttpSyncEntry(IConfigurationSection configurationSection)
        {
            Url = configurationSection[nameof(Url)];
            Secret = configurationSection[nameof(Secret)];
            Id = configurationSection[nameof(Id)];
        }

        public string Url { get; set; }
            
        public string Secret { get; set; }

        public string Id { get; set; }
    }
}