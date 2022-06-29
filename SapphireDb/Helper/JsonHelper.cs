using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using SapphireDb.Command;
using SapphireDb.Command.Error;
using SapphireDb.Internal.Prefilter;
using SapphireDb.Models;
using SapphireDb.Models.Exceptions;

namespace SapphireDb.Helper
{
    public static class JsonHelper
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public static readonly JsonSerializer DefaultSerializer = JsonSerializer.Create(Settings);
        
        public static readonly JsonSerializerSettings DeserializeCommandSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = { new SapphireJsonConverter<IPrefilterBase>(), new SapphireJsonConverter<CommandBase>() }
        };

        public static readonly JsonSerializer CommandDeserializer = JsonSerializer.Create(DeserializeCommandSettings);

        public static JToken Serialize(object value)
        {
            return JToken.FromObject(value, DefaultSerializer);
        }

        public static CommandBase DeserializeCommand(JObject value)
        {
            try
            {
                return value.ToObject<CommandBase>(CommandDeserializer);
            }
            catch (Exception ex)
            {
                string referenceId = null;

                if (value.TryGetValue("ReferenceId", out JToken referenceIdToken))
                {
                    referenceId = referenceIdToken.ToObject<string>();
                }
                
                if (ex.InnerException is SapphireDbException sapphireDbException)
                {
                    return new ErrorCommand()
                    {
                        Exception = sapphireDbException,
                        ReferenceId = referenceId
                    };
                }
                
                return new ErrorCommand()
                {
                    Exception = ex,
                    ReferenceId = referenceId
                };;
            }
        }

        public static object ToObject(this JToken jToken, Type targetType, IServiceProvider serviceProvider)
        {
            CustomModelJsonSerializer customModelJsonSerializer =
                serviceProvider.GetService<CustomModelJsonSerializer>();

            if (customModelJsonSerializer != null)
            {
                return jToken.ToObject(targetType, customModelJsonSerializer.JsonSerializer);
            }

            return jToken.ToObject(targetType);
        }
    }

    class SapphireJsonConverter<T> : JsonConverter
    {
        private readonly Dictionary<string, Type> nameTypeMappings = new Dictionary<string, Type>();

        public SapphireJsonConverter()
        {
            if (typeof(T) == typeof(IPrefilterBase))
            {
                nameTypeMappings = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(t => t.Namespace == "SapphireDb.Internal.Prefilter" && t.Name.EndsWith("Prefilter"))
                    .ToDictionary(t => t.Name, t => t);
            }
            else if (typeof(T) == typeof(CommandBase))
            {
                nameTypeMappings = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(t => typeof(CommandBase).IsAssignableFrom(t) && t.Name.EndsWith("Command"))
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

            string key = typeof(T) == typeof(IPrefilterBase) ? "prefilterType" : "commandType";

            string typeString = jObject[key].Value<string>();

            if (nameTypeMappings.ContainsKey(typeString))
            {
                return jObject.ToObject(nameTypeMappings[typeString], serializer);
            }

            return null;
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { }
    }
}
