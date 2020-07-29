using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
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
        
        public static object[] CreateParametersWithJTokensAndQueryBuilder(this MethodInfo mi, HttpInformation httpInformation, JToken[] jTokens, object queryBuilder, IServiceProvider serviceProvider)
        {
            Type queryBuilderType = queryBuilder.GetType();
            
            ParameterInfo[] parameters = mi.GetParameters();
            return parameters.Select(p =>
            {
                if (p.ParameterType == typeof(HttpInformation))
                {
                    return httpInformation;
                }

                if (p.ParameterType == typeof(JToken[]))
                {
                    return jTokens;
                }

                if (p.ParameterType == queryBuilderType)
                {
                    return queryBuilder;
                }

                return serviceProvider.GetService(p.ParameterType);
            }).ToArray();
        }
    }
}
