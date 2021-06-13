using Newtonsoft.Json.Linq;
using SapphireDb.Attributes;

namespace WebUI.Data.AuthDemo
{
    [Updateable]
    [UpdateAuth("requireUser")]
    public class UpdateExample : Base
    {
        public string RequiresUser { get; set; }

        [UpdateAuth(functionName: nameof(IsAllowed))]
        public string CustomFunction { get; set; }

        private bool IsAllowed(JObject newValue)
        {
            return RequiresUser == "Yes";
        }
        
        [UpdateAuth("requireAdmin")]
        public string RequiresAdmin { get; set; }
    }
}