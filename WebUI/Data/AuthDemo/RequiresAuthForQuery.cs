using SapphireDb.Attributes;

namespace WebUI.Data.AuthDemo
{
    [QueryAuth]
    public class RequiresAuthForQuery : Base
    {
        public string Content { get; set; }
    }
}