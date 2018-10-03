using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Prefilter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RealtimeDatabase.Internal
{
    static class JsonHelper
    {
        public static readonly JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public static readonly JsonSerializerSettings deserializeCommandSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = { new CommandConverter(), new PrefilterConverter() }
        };

        public static string Serialize(object value)
        {
            return JsonConvert.SerializeObject(value, settings);
        }

        public static object Deserialze(string value, Type t)
        {
            return JsonConvert.DeserializeObject(value, t, settings);
        }

        public static CommandBase DeserialzeCommand(string value)
        {
            return JsonConvert.DeserializeObject<CommandBase>(value, deserializeCommandSettings);
        }
    }

    class PrefilterConverter : JsonConverter
    {
        private readonly Dictionary<string, Type> NameTypeMappings = new Dictionary<string, Type>();

        public PrefilterConverter()
        {
            NameTypeMappings = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.Namespace == "RealtimeDatabase.Models.Prefilter" && t.Name.EndsWith("Prefilter"))
                .ToDictionary(t => t.Name, t => t);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IPrefilter);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);

            string commandType = jObject["prefilterType"].Value<string>();

            if (NameTypeMappings.ContainsKey(commandType))
            {
                return jObject.ToObject(NameTypeMappings[commandType], serializer);
            }

            return null;
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    class CommandConverter : JsonConverter
    {
        private readonly Dictionary<string, Type> NameTypeMappings = new Dictionary<string, Type>();

        public CommandConverter()
        {
            NameTypeMappings = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.Namespace == "RealtimeDatabase.Models.Commands" && t.Name.EndsWith("Command"))
                .ToDictionary(t => t.Name, t => t);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(CommandBase);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);

            string commandType = jObject["commandType"].Value<string>();

            if (NameTypeMappings.ContainsKey(commandType))
            {
                return jObject.ToObject(NameTypeMappings[commandType], serializer);
            }

            return null;
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
