﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using SapphireDb.Command;
using SapphireDb.Command.Error;
using SapphireDb.Internal.Prefilter;
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

        public static readonly JsonSerializerSettings DeserializeCommandSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = { new CustomJsonConverter<IPrefilterBase>(), new CustomJsonConverter<CommandBase>() }
        };

        public static string Serialize(object value)
        {
            return JsonConvert.SerializeObject(value, Settings);
        }

        public static object Deserialize(string value, Type t)
        {
            try
            {
                return JsonConvert.DeserializeObject(value, t, Settings);
            }
            catch
            {
                return null;
            }
        }

        public static CommandBase DeserializeCommand(string value)
        {
            try
            {
                return JsonConvert.DeserializeObject<CommandBase>(value, DeserializeCommandSettings);
            }
            catch (Exception ex)
            {
                string referenceId = value.TryGetReferenceId();
                
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
    }

    class CustomJsonConverter<T> : JsonConverter
    {
        private readonly Dictionary<string, Type> nameTypeMappings = new Dictionary<string, Type>();

        public CustomJsonConverter()
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
