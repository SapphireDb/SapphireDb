using System.Collections.Generic;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using SapphireDb.Attributes;
using SapphireDb.Models;
using SapphireDb.Models.SapphireApiBuilder;

namespace WebUI.Data.DemoDb
{
    [Updateable]
    [Query("only_test", nameof(OnyTestQuery))]
    [Query("test_in", nameof(TestInQuery))]
    // [DisableCreate]
    // [DisableUpdate]
    // [DisableDelete]
    // [DisableQuery]
    public class DemoEntry : SapphireOfflineEntity
    {
        public string Content { get; set; }

        public int IntegerValue { get; set; } = RandomNumberGenerator.GetInt32(1, 100);

        private static SapphireQueryBuilderBase<DemoEntry> OnyTestQuery(SapphireQueryBuilder<DemoEntry> queryBuilder, HttpInformation httpInformation, JToken[] parameters)
        {
            return queryBuilder
                .Where(e => e.Content.Length > 5)
                .OrderBy(e => e.Content)
                .ThenOrderBy(e => e.IntegerValue)
                .Select(m => new { m.Content });
        }
        
        private static SapphireQueryBuilderBase<DemoEntry> TestInQuery(SapphireQueryBuilder<DemoEntry> queryBuilder, JToken[] parameters)
        {
            List<string> values = parameters[0].ToObject<List<string>>();

            return queryBuilder
                .Where(e => values.Contains(e.Content));
        }
    }
}
