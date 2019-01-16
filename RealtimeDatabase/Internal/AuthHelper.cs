using RealtimeDatabase.Attributes;
using RealtimeDatabase.Models.Actions;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;

namespace RealtimeDatabase.Internal
{
    static class AuthHelper
    {
        public static bool CanQuery(this Type t, WebsocketConnection connection, object entityObject)
        {
            return t.HandleAuthAttribute<QueryAuthAttribute>(connection, entityObject);
        }

        public static bool CanQuery(this PropertyInfo pi, WebsocketConnection connection, object entityObject)
        {
            return pi.HandleAuthAttribute<QueryAuthAttribute>(connection, entityObject);
        }

        public static bool CanCreate(this Type t, WebsocketConnection connection, object entityObject)
        {
            return t.HandleAuthAttribute<CreateAuthAttribute>(connection, entityObject);
        }

        public static bool CanRemove(this Type t, WebsocketConnection connection, object entityObject)
        {
            return t.HandleAuthAttribute<RemoveAuthAttribute>(connection, entityObject);
        }

        public static bool CanUpdate(this Type t, WebsocketConnection connection, object entityObject)
        {
            return t.HandleAuthAttribute<UpdateAuthAttribute>(connection, entityObject);
        }

        public static bool CanUpdate(this PropertyInfo pi, WebsocketConnection connection, object entityObject)
        {
            return pi.HandleAuthAttribute<UpdateAuthAttribute>(connection, entityObject);
        }

        public static object GetAuthenticatedQueryModel(this object model, WebsocketConnection websocketConnection)
        {
            PropertyInfo[] propertyInfos = model.GetType().GetProperties();

            if (!propertyInfos.Where(pi => pi.GetCustomAttribute<QueryAuthAttribute>() != null).Any())
            {
                return model;
            }

            Dictionary<string, object> value = new Dictionary<string, object>();

            foreach (PropertyInfo pi in propertyInfos)
            {
                if (pi.CanQuery(websocketConnection, model))
                {
                    value.Add(pi.Name.ToCamelCase(), pi.GetValue(model));
                }
            }

            return value;
        }

        public static bool CanExecuteAction(this Type type, WebsocketConnection websocketConnection, ActionHandlerBase actionHandler)
        {
            return type.HandleAuthAttribute<ActionAuthAttribute>(websocketConnection, actionHandler);
        }

        public static bool CanExecuteAction(this MethodInfo methodInfo, WebsocketConnection websocketConnection, ActionHandlerBase actionHandler)
        {
            ActionAuthAttribute authAttribute = methodInfo.GetCustomAttribute<ActionAuthAttribute>();
            return HandleAuthAttribute(methodInfo.DeclaringType, authAttribute, websocketConnection, actionHandler);
        }

        private static bool HandleAuthAttribute(Type t, AuthAttributeBase authAttribute, WebsocketConnection connection, object entityObject)
        {
            if (authAttribute == null)
            {
                return true;
            }

            ClaimsPrincipal user = connection.HttpContext.User;

            if (user.Identity.IsAuthenticated)
            {
                if (authAttribute.Roles != null)
                {
                    return authAttribute.Roles.Any(r => user.IsInRole(r));
                }
                else if (!String.IsNullOrEmpty(authAttribute.FunctionName))
                {
                    MethodInfo mi = t.GetMethod(authAttribute.FunctionName);
                    if (mi != null && mi.ReturnType == typeof(bool)
                        && mi.GetParameters().Count() == 1
                        && mi.GetParameters()[0].ParameterType == typeof(WebsocketConnection))
                    {
                        return (bool)mi.Invoke(entityObject, new object[] { connection });
                    }
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HandleAuthAttribute<T>(this Type t, WebsocketConnection connection, object entityObject) where T : AuthAttributeBase
        {
            AuthAttributeBase authAttribute = t.GetCustomAttribute<T>();
            return HandleAuthAttribute(t, authAttribute, connection, entityObject);
        }

        private static bool HandleAuthAttribute<T>(this PropertyInfo pi, WebsocketConnection connection, object entityObject) where T : AuthAttributeBase
        {
            AuthAttributeBase authAttribute = pi.GetCustomAttribute<T>();
            return HandleAuthAttribute(pi.DeclaringType, authAttribute, connection, entityObject);
        }
    }
}
