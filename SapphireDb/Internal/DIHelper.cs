using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Http;
using SapphireDb.Models;

namespace SapphireDb.Internal
{
    static class DiHelper
    {
        public static object[] CreateParameters(this MethodInfo mi, HttpInformation httpInformation, IServiceProvider serviceProvider)
        {
            ParameterInfo[] parameters = mi.GetParameters();
            return parameters.Select(p =>
            {
                if (p.ParameterType == typeof(HttpInformation))
                {
                    return httpInformation;
                }

                return serviceProvider.GetService(p.ParameterType);
            }).ToArray();
        }
    }
}
