using SapphireDb.Attributes;
using SapphireDb.Models.SapphireApiBuilder;

namespace WebUI.Data.DemoDb
{
    [Query("by_content", nameof(ByContentQuery))]
    public class ServerSideQueryWithDefault : ServerSideQueryWithDefaultBase
    {
        public string Content { get; set; }
        
        private static SapphireQueryBuilder<ServerSideQueryWithDefault> ByContentQuery(
            SapphireQueryBuilder<ServerSideQueryWithDefault> builder)
        {
            return builder
                .Where(entry => entry.Content.Length > 5);
        }
    }
}