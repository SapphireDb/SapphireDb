using System;
using System.Reflection;
using Newtonsoft.Json.Linq;
using SapphireDb.Models;

namespace SapphireDb.Attributes
{
    public abstract class QueryAttributeBase : Attribute
    {
        public string FunctionName { get; set; }

        public MethodInfo FunctionInfo { get; set; }
        
        public Func<dynamic, HttpInformation, JToken[], dynamic> FunctionLambda { get; set; }
    }
}