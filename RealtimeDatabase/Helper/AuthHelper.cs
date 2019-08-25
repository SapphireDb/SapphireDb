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
using RealtimeDatabase.Models.Auth;
using RealtimeDatabase.Models.Prefilter;

namespace RealtimeDatabase.Helper
{
    static class AuthHelper
    {
        public static bool CanQuery(this Type t, HttpContext context, object entityObject,
            IServiceProvider serviceProvider)
        {
            return t.HandleAuthAttribute<QueryAuthAttribute>(context, entityObject, serviceProvider);
        }

        public static bool CanQuery(this AuthPropertyInfo pi, HttpContext context, object entityObject,
            IServiceProvider serviceProvider)
        {
            return HandleAuthAttribute(pi.PropertyInfo.DeclaringType, pi.QueryAuthAttribute, context, entityObject, serviceProvider);
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

        public static bool CanUpdate(this AuthPropertyInfo pi, HttpContext context, object entityObject,
            IServiceProvider serviceProvider)
        {
            return HandleAuthAttribute(pi.PropertyInfo.DeclaringType, pi.UpdateAuthAttribute, context, entityObject, serviceProvider);
        }

        public static object GetAuthenticatedQueryModel(this object model, HttpContext context,
            IServiceProvider serviceProvider)
        {
            AuthPropertyInfo[] propertyInfos = model.GetType().GetAuthPropertyInfos();

            if (propertyInfos.All(pi => pi.QueryAuthAttribute == null))
            {
                return model;
            }

            Dictionary<string, object> value = new Dictionary<string, object>();

            foreach (AuthPropertyInfo pi in propertyInfos)
            {
                if (pi.CanQuery(context, model, serviceProvider))
                {
                    value.Add(pi.PropertyInfo.Name.ToCamelCase(), pi.PropertyInfo.GetValue(model));
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

            if (authAttribute.Roles != null)
            {
                return user.Identity.IsAuthenticated && authAttribute.Roles.Any(r => user.IsInRole(r));
            }

            if (!string.IsNullOrEmpty(authAttribute.FunctionName))
            {
                MethodInfo mi = t.GetMethod(authAttribute.FunctionName);
                if (mi != null && mi.ReturnType == typeof(bool))
                {
                    return (bool) mi.Invoke(entityObject, mi.CreateParameters(context, serviceProvider));
                }
            }

            return user.Identity.IsAuthenticated;
        }

        private static bool HandleAuthAttribute<T>(this Type t, HttpContext context, object entityObject,
            IServiceProvider serviceProvider) where T : AuthAttributeBase
        {
            AuthAttributeBase authAttribute = t.GetCustomAttribute<T>();
            return HandleAuthAttribute(t, authAttribute, context, entityObject, serviceProvider);
        }
    }
}
