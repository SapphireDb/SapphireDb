using SapphireDb.Attributes;

namespace WebUI.Data.AuthDemo
{
    [QueryAuth("requireAdmin")]
    public class RequiresAdminForQuery : Base
    {
        public string Content { get; set; }
    }
}