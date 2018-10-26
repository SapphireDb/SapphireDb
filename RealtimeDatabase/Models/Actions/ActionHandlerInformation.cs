using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Models.Actions
{
    public class ActionHandlerInformation
    {
        public string Name { get; set; }

        public Type Type { get; set; }

        public ActionHandlerInformation(string name, Type type)
        {
            Name = name;
            Type = type;
        }
    }
}
