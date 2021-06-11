using SapphireDb.Attributes;
using SapphireDb.Models;
using SapphireDb.Models.SapphireApiBuilder;

namespace WebUI.Data.DemoDb
{
    [DefaultQuery(nameof(DefaultQuery))]
    [DeleteEvent(insteadOf: nameof(InsteadOfDelete))]
    public class ServerSideQueryWithDefaultBase : SapphireOfflineEntity
    {
        public bool Deleted { get; set; }

        private static SapphireQueryBuilder<T> DefaultQuery<T>(SapphireQueryBuilder<T> builder)
            where T : ServerSideQueryWithDefaultBase
        {
            return builder
                .Where(entry => !entry.Deleted);
        }

        private void InsteadOfDelete()
        {
            Deleted = true;
        }
    }
}