using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RealtimeDatabase.Websocket.Models;

namespace RealtimeDatabase.Internal
{
    static class DIHelper
    {
        public static object[] CreateParameters(this MethodInfo mi, WebsocketConnection connection, IServiceProvider serviceProvider)
        {
            ParameterInfo[] parameters = mi.GetParameters();
            return parameters.Select(p =>
            {
                if (p.ParameterType == typeof(WebsocketConnection))
                {
                    return connection;
                }

                return serviceProvider.GetService(p.ParameterType);
            }).ToArray();
        }
    }
}
