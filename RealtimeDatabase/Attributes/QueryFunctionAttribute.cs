using System;

namespace RealtimeDatabase.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class QueryFunctionAttribute : Attribute
    {
        public string Function { get; set; }

        public QueryFunctionAttribute(string function)
        {
            Function = function;
        }
    }
}
