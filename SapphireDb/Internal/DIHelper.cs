using System;
using System.Linq;
using System.Reflection;
using SapphireDb.Connection;
using SapphireDb.Models;

namespace SapphireDb.Internal
{
    static class DiHelper
    {
        public static object[] CreateParameters(this MethodInfo mi, IConnectionInformation connectionInformation, IServiceProvider serviceProvider, params object[] additionalParameters)
        {
            ParameterInfo[] parameters = mi.GetParameters();
            return parameters.Select(p =>
            {
                if (p.ParameterType == typeof(IConnectionInformation))
                {
                    return connectionInformation;
                }

                object additionalParameter = additionalParameters.FirstOrDefault(parameter => parameter?.GetType() == p.ParameterType);
                if (additionalParameter != null)
                {
                    return additionalParameter;
                }

                return serviceProvider.GetService(p.ParameterType);
            }).ToArray();
        }
    }
}
