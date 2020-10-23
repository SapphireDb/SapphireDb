using SapphireDb.Attributes;
using SapphireDb.Models;

namespace WebUI.Data.AuthDemo
{
    public class QueryFields : Base
    {
        [QueryAuth]
        [Updateable]
        public string Content { get; set; }
        
        [QueryAuth("requireAdmin")]
        public string Content2 { get; set; }

        [QueryAuth(functionName: "CanQuery")]
        public string Content3 { get; set; }

        private bool CanQuery()
        {
            return Content == "Test 1";
        }
    }
}