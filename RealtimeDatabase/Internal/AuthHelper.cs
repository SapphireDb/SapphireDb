using RealtimeDatabase.Attributes;
using RealtimeDatabase.Models.Actions;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using RealtimeDatabase.Models;

namespace RealtimeDatabase.Internal
{
    static class AuthHelper
    {
        public static bool CanQuery(this Type t, WebsocketConnection connection, object entityObject,
            IServiceProvider serviceProvider)
        {
            return t.HandleAuthAttribute<QueryAuthAttribute>(connection, entityObject, serviceProvider);
        }

        public static bool CanQuery(this PropertyInfo pi, WebsocketConnection connection, object entityObject,
            IServiceProvider serviceProvider)
        {
            return pi.HandleAuthAttribute<QueryAuthAttribute>(connection, entityObject, serviceProvider);
        }

        public static bool CanCreate(this Type t, WebsocketConnection connection, object entityObject,
            IServiceProvider serviceProvider)
        {
            return t.HandleAuthAttribute<CreateAuthAttribute>(connection, entityObject, serviceProvider);
        }

        public static bool CanRemove(this Type t, WebsocketConnection connection, object entityObject,
            IServiceProvider serviceProvider)
        {
            return t.HandleAuthAttribute<RemoveAuthAttribute>(connection, entityObject, serviceProvider);
        }

        public static bool CanUpdate(this Type t, WebsocketConnection connection, object entityObject,
            IServiceProvider serviceProvider)
        {
            return t.HandleAuthAttribute<UpdateAuthAttribute>(connection, entityObject, serviceProvider);
        }

        public static bool CanUpdate(this PropertyInfo pi, WebsocketConnection connection, object entityObject,
            IServiceProvider serviceProvider)
        {
            return pi.HandleAuthAttribute<UpdateAuthAttribute>(connection, entityObject, serviceProvider);
        }

        public static object GetAuthenticatedQueryModel(this object model, WebsocketConnection websocketConnection,
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
                if (pi.CanQuery(websocketConnection, model, serviceProvider))
                {
                    value.Add(pi.Name.ToCamelCase(), pi.GetValue(model));
                }
            }

            return value;
        }

        public static bool CanExecuteAction(this Type type, WebsocketConnection websocketConnection,
            ActionHandlerBase actionHandler, IServiceProvider serviceProvider)
        {
            return type.HandleAuthAttribute<ActionAuthAttribute>(websocketConnection, actionHandler, serviceProvider);
        }

        public static bool CanExecuteAction(this MethodInfo methodInfo, WebsocketConnection websocketConnection,
            ActionHandlerBase actionHandler, IServiceProvider serviceProvider)
        {
            ActionAuthAttribute authAttribute = methodInfo.GetCustomAttribute<ActionAuthAttribute>();
            return HandleAuthAttribute(methodInfo.DeclaringType, authAttribute, websocketConnection, actionHandler, serviceProvider);
        }

        private static bool HandleAuthAttribute(Type t, AuthAttributeBase authAttribute, WebsocketConnection connection, object entityObject, IServiceProvider serviceProvider)
        {
            if (authAttribute == null)
            {
                return true;
            }

            ClaimsPrincipal user = connection.HttpContext.User;
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
                    return (bool) mi.Invoke(entityObject, mi.CreateParameters(connection, serviceProvider));
                }
            }

            return true;
        }

        private static bool HandleAuthAttribute<T>(this Type t, WebsocketConnection connection, object entityObject,
            IServiceProvider serviceProvider) where T : AuthAttributeBase
        {
            AuthAttributeBase authAttribute = t.GetCustomAttribute<T>();
            return HandleAuthAttribute(t, authAttribute, connection, entityObject, serviceProvider);
        }

        private static bool HandleAuthAttribute<T>(this PropertyInfo pi, WebsocketConnection connection,
            object entityObject, IServiceProvider serviceProvider) where T : AuthAttributeBase
        {
            AuthAttributeBase authAttribute = pi.GetCustomAttribute<T>();
            return HandleAuthAttribute(pi.DeclaringType, authAttribute, connection, entityObject, serviceProvider);
        }
    }
}
