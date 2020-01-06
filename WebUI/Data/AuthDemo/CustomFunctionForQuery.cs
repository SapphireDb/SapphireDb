using SapphireDb.Attributes;
using SapphireDb.Models;

namespace WebUI.Data.AuthDemo
{
    [QueryAuth(functionName: "CanQuery")]
    [QueryAuth(functionName: "CanQuery2")]
    public class CustomFunctionForQuery : Base
    {
        public static bool CanQuery(HttpInformation httpInformation)
        {
            return httpInformation.User.IsInRole("admin");
        }
        
        public static bool CanQuery2(HttpInformation httpInformation)
        {
            return httpInformation.User.IsInRole("user");
        }
        
        public string Content { get; set; }
    }
}