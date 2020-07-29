using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using SapphireDb.Attributes;
using SapphireDb.Models;
using SapphireDb.Models.SapphireApiBuilder;

namespace WebUI.Data.DemoDb
{
    [Updateable]
    [Query("only_test", nameof(OnyTestQuery))]
    public class DemoEntry : SapphireOfflineEntity
    {
        public string Content { get; set; }

        public int IntegerValue { get; set; } = RandomNumberGenerator.GetInt32(1, 100);

        private static SapphireQueryBuilder<DemoEntry> OnyTestQuery(SapphireQueryBuilder<DemoEntry> queryBuilder, HttpInformation httpInformation, JToken[] parameters)
        {
            return queryBuilder
                .Where(d => d.Content == "Test");
        }
    }
}
