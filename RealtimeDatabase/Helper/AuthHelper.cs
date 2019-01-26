using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Attributes;
using RealtimeDatabase.Internal;
using RealtimeDatabase.Models;
using RealtimeDatabase.Models.Actions;
using RealtimeDatabase.Websocket.Models;

namespace RealtimeDatabase.Helper
{
    static class AuthHelper
    {
        public static bool CanQuery(this Type t, HttpContext context, object entityObject,
            IServiceProvider serviceProvider)
        {
            return t.HandleAuthAttribute<QueryAuthAttribute>(context, entityObject, serviceProvider);
        }

        public static bool CanQuery(this PropertyInfo pi, HttpContext context, object entityObject,
            IServiceProvider serviceProvider)
        {
            return pi.HandleAuthAttribute<QueryAuthAttribute>(context, entityObject, serviceProvider);
        }

        public static bool CanCreate(this Type t, HttpContext context, object entityObject,
            IServiceProvider serviceProvider)
        {
            return t.HandleAuthAttribute<CreateAuthAttribute>(context, entityObject, serviceProvider);
        }

        public static bool CanRemove(this Type t, HttpContext context, object entityObject,
            IServiceProvider serviceProvider)
        {
            return t.HandleAuthAttribute<RemoveAuthAttribute>(context, entityObject, serviceProvider);
        }

        public static bool CanUpdate(this Type t, HttpContext context, object entityObject,
            IServiceProvider serviceProvider)
        {
            return t.HandleAuthAttribute<UpdateAuthAttribute>(context, entityObject, serviceProvider);
        }

        public static bool CanUpdate(this PropertyInfo pi, HttpContext context, object entityObject,
            IServiceProvider serviceProvider)
        {
            return pi.HandleAuthAttribute<UpdateAuthAttribute>(context, entityObject, serviceProvider);
        }

        public static object GetAuthenticatedQueryModel(this object model, HttpContext context,
            IServiceProvider serviceProvider)
        {
            PropertyInfo[] propertyInfos = model.GetType().GetProperties();

            if (propertyInfos.All(pi => pi.GetCustomAttribute<QueryAuthAttribute>() == null))
            {
                return model;
            }

            Dictionary<string, object> value = new Dictionary<string, object>();

            foreach (PropertyInfo pi in propertyInfos)
            {
                if (pi.CanQuery(context, model, serviceProvider))
                {
                    value.Add(pi.Name.ToCamelCase(), pi.GetValue(model));
                }
            }

            return value;
        }

        public static bool CanExecuteAction(this Type type, HttpContext context,
            ActionHandlerBase actionHandler, IServiceProvider serviceProvider)
        {
            return type.HandleAuthAttribute<ActionAuthAttribute>(context, actionHandler, serviceProvider);
        }

        public static bool CanExecuteAction(this MethodInfo methodInfo, HttpContext context,
            ActionHandlerBase actionHandler, IServiceProvider serviceProvider)
        {
            ActionAuthAttribute authAttribute = methodInfo.GetCustomAttribute<ActionAuthAttribute>();
            return HandleAuthAttribute(methodInfo.DeclaringType, authAttribute, context, actionHandler, serviceProvider);
        }

        private static bool HandleAuthAttribute(Type t, AuthAttributeBase authAttribute, HttpContext context, object entityObject, IServiceProvider serviceProvider)
        {
            if (authAttribute == null)
            {
                return true;
            }

            ClaimsPrincipal user = context.User;
            RealtimeDatabaseOptions options = (RealtimeDatabaseOptions)serviceProvider.GetService(typeof(RealtimeDatabaseOptions));

            if (options.RequireAuthenticationForAttribute && !user.Identity.IsAuthenticated)
            {
                return false;
            }

            if (authAttribute.Roles != null)
            {
                return authAttribute.Roles.Any(r => user.IsInRole(r));
            }

            if (!string.IsNullOrEmpty(authAttribute.FunctionName))
            {
                MethodInfo mi = t.GetMethod(authAttribute.FunctionName);
                if (mi != null && mi.ReturnType == typeof(bool))
                {
                    return (bool) mi.Invoke(entityObject, mi.CreateParameters(context, serviceProvider));
                }
            }

            return true;
        }

        private static bool HandleAuthAttribute<T>(this Type t, HttpContext context, object entityObject,
            IServiceProvider serviceProvider) where T : AuthAttributeBase
        {
            AuthAttributeBase authAttribute = t.GetCustomAttribute<T>();
            return HandleAuthAttribute(t, authAttribute, context, entityObject, serviceProvider);
        }

        private static bool HandleAuthAttribute<T>(this PropertyInfo pi, HttpContext context,
            object entityObject, IServiceProvider serviceProvider) where T : AuthAttributeBase
        {
            AuthAttributeBase authAttribute = pi.GetCustomAttribute<T>();
            return HandleAuthAttribute(pi.DeclaringType, authAttribute, context, entityObject, serviceProvider);
        }
    }
}
