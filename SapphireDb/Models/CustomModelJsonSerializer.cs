using Newtonsoft.Json;

namespace SapphireDb.Models;

public class CustomModelJsonSerializer
{
    public JsonSerializer JsonSerializer { get; }

    public CustomModelJsonSerializer(JsonSerializer jsonSerializer)
    {
        JsonSerializer = jsonSerializer;
    }
}