using SapphireDb.Attributes;

namespace WebUI.Data.AuthDemo
{
    [Updatable]
    [UpdateAuth("requireUser")]
    public class UpdateExample : Base
    {
        public string RequiresUser { get; set; }

        [UpdateAuth(functionName: nameof(IsAllowed))]
        public string CustomFunction { get; set; }

        private bool IsAllowed()
        {
            return RequiresUser == "Yes";
        }
        
        [UpdateAuth("requireAdmin")]
        public string RequiresAdmin { get; set; }
    }
}