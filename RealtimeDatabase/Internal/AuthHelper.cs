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
            QueryAuthAttribute queryAuthAttribute = t.GetCustomAttribute<QueryAuthAttribute>();

            if (queryAuthAttribute == null)
            {
                return true;
            }

            ClaimsPrincipal user = connection.HttpContext.User;

            if (user.Identity.IsAuthenticated)
            {
                if (queryAuthAttribute.Roles != null)
                {
                    return queryAuthAttribute.Roles.Any(r => user.IsInRole(r));
                }
                else if (!String.IsNullOrEmpty(queryAuthAttribute.FunctionName))
                {
                    MethodInfo mi = t.GetMethod(queryAuthAttribute.FunctionName);
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

        public static bool CanQuery(this PropertyInfo pi, WebsocketConnection connection, object entityObject)
        {
            QueryAuthAttribute queryAuthAttribute = pi.GetCustomAttribute<QueryAuthAttribute>();

            if (queryAuthAttribute == null)
            {
                return true;
            }

            ClaimsPrincipal user = connection.HttpContext.User;

            if (user.Identity.IsAuthenticated)
            {
                if (queryAuthAttribute.Roles != null)
                {
                    return queryAuthAttribute.Roles.Any(r => user.IsInRole(r));
                }
                else if (!String.IsNullOrEmpty(queryAuthAttribute.FunctionName))
                {
                    MethodInfo mi = pi.DeclaringType.GetMethod(queryAuthAttribute.FunctionName);
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

        public static bool CanCreate(this Type t, WebsocketConnection connection, object entityObject)
        {
            CreateAuthAttribute createAuthAttribute = t.GetCustomAttribute<CreateAuthAttribute>();

            if (createAuthAttribute == null)
            {
                return true;
            }

            ClaimsPrincipal user = connection.HttpContext.User;

            if (user.Identity.IsAuthenticated)
            {
                if (createAuthAttribute.Roles != null)
                {
                    return createAuthAttribute.Roles.Any(r => user.IsInRole(r));
                }
                else if (!String.IsNullOrEmpty(createAuthAttribute.FunctionName))
                {
                    MethodInfo mi = t.GetMethod(createAuthAttribute.FunctionName);
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

        public static bool CanRemove(this Type t, WebsocketConnection connection, object entityObject)
        {
            RemoveAuthAttribute removeAuthAttribute = t.GetCustomAttribute<RemoveAuthAttribute>();

            if (removeAuthAttribute == null)
            {
                return true;
            }

            ClaimsPrincipal user = connection.HttpContext.User;

            if (user.Identity.IsAuthenticated)
            {
                if (removeAuthAttribute.Roles != null)
                {
                    return removeAuthAttribute.Roles.Any(r => user.IsInRole(r));
                }
                else if (!String.IsNullOrEmpty(removeAuthAttribute.FunctionName))
                {
                    MethodInfo mi = t.GetMethod(removeAuthAttribute.FunctionName);
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

        public static bool CanUpdate(this Type t, WebsocketConnection connection, object entityObject)
        {
            UpdateAuthAttribute updateAuthAttribute = t.GetCustomAttribute<UpdateAuthAttribute>();

            if (updateAuthAttribute == null)
            {
                return true;
            }

            ClaimsPrincipal user = connection.HttpContext.User;

            if (user.Identity.IsAuthenticated)
            {
                if (updateAuthAttribute.Roles != null)
                {
                    return updateAuthAttribute.Roles.Any(r => user.IsInRole(r));
                }
                else if (!String.IsNullOrEmpty(updateAuthAttribute.FunctionName))
                {
                    MethodInfo mi = t.GetMethod(updateAuthAttribute.FunctionName);
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

        public static bool CanUpdate(this PropertyInfo pi, WebsocketConnection connection, object entityObject)
        {
            UpdateAuthAttribute updateAuthAttribute = pi.GetCustomAttribute<UpdateAuthAttribute>();

            if (updateAuthAttribute == null)
            {
                return true;
            }

            ClaimsPrincipal user = connection.HttpContext.User;

            if (user.Identity.IsAuthenticated)
            {
                if (updateAuthAttribute.Roles != null)
                {
                    return updateAuthAttribute.Roles.Any(r => user.IsInRole(r));
                }
                else if (!String.IsNullOrEmpty(updateAuthAttribute.FunctionName))
                {
                    MethodInfo mi = pi.DeclaringType.GetMethod(updateAuthAttribute.FunctionName);
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
            ActionAuthAttribute authAttribute = type.GetCustomAttribute<ActionAuthAttribute>();

            if (authAttribute == null)
            {
                return true;
            }

            ClaimsPrincipal user = websocketConnection.HttpContext.User;

            if (user.Identity.IsAuthenticated)
            {
                if (authAttribute.Roles != null)
                {
                    return authAttribute.Roles.Any(r => user.IsInRole(r));
                }
                else if (!String.IsNullOrEmpty(authAttribute.FunctionName))
                {
                    MethodInfo mi = type.GetMethod(authAttribute.FunctionName);
                    if (mi != null && mi.ReturnType == typeof(bool)
                        && mi.GetParameters().Count() == 1
                        && mi.GetParameters()[0].ParameterType == typeof(WebsocketConnection))
                    {
                        return (bool)mi.Invoke(actionHandler, new object[] { websocketConnection });
                    }
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        public static bool CanExecuteAction(this MethodInfo methodInfo, WebsocketConnection websocketConnection, ActionHandlerBase actionHandler)
        {
            ActionAuthAttribute authAttribute = methodInfo.GetCustomAttribute<ActionAuthAttribute>();

            if (authAttribute == null)
            {
                return true;
            }

            ClaimsPrincipal user = websocketConnection.HttpContext.User;

            if (user.Identity.IsAuthenticated)
            {
                if (authAttribute.Roles != null)
                {
                    return authAttribute.Roles.Any(r => user.IsInRole(r));
                }
                else if (!String.IsNullOrEmpty(authAttribute.FunctionName))
                {
                    MethodInfo mi = methodInfo.DeclaringType.GetMethod(authAttribute.FunctionName);
                    if (mi != null && mi.ReturnType == typeof(bool)
                        && mi.GetParameters().Count() == 1
                        && mi.GetParameters()[0].ParameterType == typeof(WebsocketConnection))
                    {
                        return (bool)mi.Invoke(actionHandler, new object[] { websocketConnection });
                    }
                }
                else
                {
                    return true;
                }
            }

            return false;
        }
    }
}
