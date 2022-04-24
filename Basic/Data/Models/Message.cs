using SapphireDb.Attributes;

namespace Basic.Data.Models;

public class Message : Base
{
    [Updateable]
    public string Content { get; set; }
}