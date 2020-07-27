using System.Security.Cryptography;
using SapphireDb.Attributes;
using SapphireDb.Models;

namespace WebUI.Data.DemoDb
{
    [Updateable]
    public class DemoEntry : SapphireOfflineEntity
    {
        public string Content { get; set; }

        public int IntegerValue { get; set; } = RandomNumberGenerator.GetInt32(1, 100);
    }
}
