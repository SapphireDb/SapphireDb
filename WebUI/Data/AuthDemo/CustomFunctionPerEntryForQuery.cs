using SapphireDb.Attributes;
using SapphireDb.Models;

namespace WebUI.Data.AuthDemo
{
    [QueryEntryAuth(functionName: "CanQuery")]
    [QueryEntryAuth(functionName: "CanQuery2")]
    public class CustomFunctionPerEntryForQuery : Base
    {
        public bool CanQuery(HttpInformation httpInformation)
        {
            return Content == "Test 1";
        }

        public bool CanQuery2(HttpInformation httpInformation)
        {
            return Content == "Test 2";
        }
        
        [Updatable]
        public string Content { get; set; }
    }
}