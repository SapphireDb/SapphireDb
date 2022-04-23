using System;
using System.Reflection;
using Newtonsoft.Json.Linq;
using SapphireDb.Connection;
using SapphireDb.Models;

namespace SapphireDb.Attributes
{
    public abstract class QueryAttributeBase : Attribute
    {
        public string FunctionName { get; set; }

        public MethodInfo FunctionInfo { get; set; }
        
        public Func<dynamic, IConnectionInformation, JToken[], dynamic> FunctionLambda { get; set; }
    }
}