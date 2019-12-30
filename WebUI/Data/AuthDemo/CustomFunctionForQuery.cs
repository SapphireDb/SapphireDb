using SapphireDb.Attributes;
using SapphireDb.Models;

namespace WebUI.Data.AuthDemo
{
    [QueryAuth(functionName: "CanQuery")]
    public class CustomFunctionForQuery : Base
    {
        public static bool CanQuery(HttpInformation httpInformation)
        {
            return httpInformation.User.IsInRole("admin");
        }
        
        public string Content { get; set; }
    }
}