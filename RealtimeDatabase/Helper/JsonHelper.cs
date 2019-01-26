using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Prefilter;

namespace RealtimeDatabase.Helper
{
    static class JsonHelper
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public static readonly JsonSerializerSettings DeserializeCommandSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = { new CustomJsonConverter<IPrefilter>(), new CustomJsonConverter<CommandBase>() }
        };

        public static string Serialize(object value)
        {
            return JsonConvert.SerializeObject(value, Settings);
        }

        public static object Deserialze(string value, Type t)
        {
            return JsonConvert.DeserializeObject(value, t, Settings);
        }

        public static CommandBase DeserialzeCommand(string value)
        {
            return JsonConvert.DeserializeObject<CommandBase>(value, DeserializeCommandSettings);
        }
    }

    class CustomJsonConverter<T> : JsonConverter
    {
        private readonly Dictionary<string, Type> NameTypeMappings = new Dictionary<string, Type>();

        public CustomJsonConverter()
        {
            if (typeof(T) == typeof(IPrefilter))
            {
                NameTypeMappings = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(t => t.Namespace == "RealtimeDatabase.Models.Prefilter" && t.Name.EndsWith("Prefilter"))
                    .ToDictionary(t => t.Name, t => t);
            }
            else if (typeof(T) == typeof(CommandBase))
            {
                NameTypeMappings = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(t => t.Namespace == "RealtimeDatabase.Models.Commands" && t.Name.EndsWith("Command"))
                    .ToDictionary(t => t.Name, t => t);
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(T);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);

            string key = typeof(T) == typeof(IPrefilter) ? "prefilterType" : "commandType";

            string typeString = jObject[key].Value<string>();

            if (NameTypeMappings.ContainsKey(typeString))
            {
                return jObject.ToObject(NameTypeMappings[typeString], serializer);
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
