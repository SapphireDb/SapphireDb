using RealtimeDatabase.Attributes;
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
        public static bool CanQuery(this Type t, WebsocketConnection connection)
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
                else
                {
                    return true;
                }
            }

            return false;
        }

        public static bool CanQuery(this PropertyInfo pi, WebsocketConnection connection)
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
                else
                {
                    return true;
                }
            }

            return false;
        }

        public static bool CanCreate(this Type t, WebsocketConnection connection)
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
                else
                {
                    return true;
                }
            }

            return false;
        }

        public static bool CanRemove(this Type t, WebsocketConnection connection)
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
                else
                {
                    return true;
                }
            }

            return false;
        }

        public static bool CanUpdate(this Type t, WebsocketConnection connection)
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
                else
                {
                    return true;
                }
            }

            return false;
        }

        public static bool CanUpdate(this PropertyInfo pi, WebsocketConnection connection)
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
                if (pi.CanQuery(websocketConnection))
                {
                    value.Add(pi.Name.ToCamelCase(), pi.GetValue(model));
                }
            }

            return value;
        }
    }
}
