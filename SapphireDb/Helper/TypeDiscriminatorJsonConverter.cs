using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SapphireDb.Attributes;

namespace SapphireDb.Helper;

public class TypeDiscriminatorJsonConverter<T> : JsonConverter
{
    private readonly List<TypeDiscriminatorAttribute> typeDiscriminators;
        
    public TypeDiscriminatorJsonConverter()
    {
        typeDiscriminators = typeof(T).GetCustomAttributes<TypeDiscriminatorAttribute>().ToList();
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(T);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject jObject = JObject.Load(reader);

        string typeString = jObject.GetValue("type", StringComparison.InvariantCultureIgnoreCase)?.Value<string>();

        TypeDiscriminatorAttribute discriminator = typeDiscriminators.FirstOrDefault(d => d.TypeName == typeString);
            
        if (discriminator != null)
        {
            return jObject.ToObject(discriminator.Type, serializer);
        }

        return null;
    }

    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { }
}