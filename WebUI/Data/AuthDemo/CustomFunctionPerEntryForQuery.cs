using SapphireDb.Attributes;
using SapphireDb.Models;

namespace WebUI.Data.AuthDemo
{
    [QueryAuth("requireAdmin", "CanQuery", true)]
    public class CustomFunctionPerEntryForQuery : Base
    {
        public bool CanQuery(HttpInformation httpInformation)
        {
            return Content == "Test 1";
        }

        [Updatable]
        public string Content { get; set; }
    }
}