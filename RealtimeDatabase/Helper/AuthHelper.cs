using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Actions;
using RealtimeDatabase.Attributes;
using RealtimeDatabase.Internal;
using RealtimeDatabase.Models;
using RealtimeDatabase.Models.Auth;

namespace RealtimeDatabase.Helper
{
    static class AuthHelper
    {
        public static bool CheckApiAuth(string key, string secret, RealtimeDatabaseOptions options)
        {
            return !options.ApiConfigurations.Any() || options.ApiConfigurations.Any((config) => config.Key == key && config.Secret == secret.ComputeHash());
        }

        public static bool CanQuery(this Type t, HttpInformation httpInformation, object entityObject,
            IServiceProvider serviceProvider)
        {
            return t.HandleAuthAttribute<QueryAuthAttribute>(httpInformation, entityObject, serviceProvider);
        }

        public static bool CanQuery(this AuthPropertyInfo pi, HttpInformation httpInformation, object entityObject,
            IServiceProvider serviceProvider)
        {
            return HandleAuthAttribute(pi.PropertyInfo.DeclaringType, pi.QueryAuthAttribute, httpInformation, entityObject, serviceProvider);
        }

        public static bool CanCreate(this Type t, HttpInformation httpInformation, object entityObject,
            IServiceProvider serviceProvider)
        {
            return t.HandleAuthAttribute<CreateAuthAttribute>(httpInformation, entityObject, serviceProvider);
        }

        public static bool CanRemove(this Type t, HttpInformation httpInformation, object entityObject,
            IServiceProvider serviceProvider)
        {
            return t.HandleAuthAttribute<RemoveAuthAttribute>(httpInformation, entityObject, serviceProvider);
        }

        public static bool CanUpdate(this Type t, HttpInformation httpInformation, object entityObject,
            IServiceProvider serviceProvider)
        {
            return t.HandleAuthAttribute<UpdateAuthAttribute>(httpInformation, entityObject, serviceProvider);
        }

        public static bool CanUpdate(this AuthPropertyInfo pi, HttpInformation httpInformation, object entityObject,
            IServiceProvider serviceProvider)
        {
            return HandleAuthAttribute(pi.PropertyInfo.DeclaringType, pi.UpdateAuthAttribute, httpInformation, entityObject, serviceProvider);
        }

        public static object GetAuthenticatedQueryModel(this object model, HttpInformation httpInformation,
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
                if (pi.CanQuery(httpInformation, model, serviceProvider))
                {
                    value.Add(pi.PropertyInfo.Name.ToCamelCase(), pi.PropertyInfo.GetValue(model));
                }
            }

            return value;
        }

        public static bool CanExecuteAction(this Type type, HttpInformation httpInformation,
            ActionHandlerBase actionHandler, IServiceProvider serviceProvider)
        {
            return type.HandleAuthAttribute<ActionAuthAttribute>(httpInformation, actionHandler, serviceProvider);
        }

        public static bool CanExecuteAction(this MethodInfo methodInfo, HttpInformation httpInformation,
            ActionHandlerBase actionHandler, IServiceProvider serviceProvider)
        {
            ActionAuthAttribute authAttribute = methodInfo.GetCustomAttribute<ActionAuthAttribute>();
            return HandleAuthAttribute(methodInfo.DeclaringType, authAttribute, httpInformation, actionHandler, serviceProvider);
        }

        private static bool HandleAuthAttribute(Type t, AuthAttributeBase authAttribute, HttpInformation httpInformation, object entityObject, IServiceProvider serviceProvider)
        {
            if (authAttribute == null)
            {
                return true;
            }

            ClaimsPrincipal user = httpInformation.User;

            if (authAttribute.Roles != null)
            {
                return user.Identity.IsAuthenticated && authAttribute.Roles.Any(r => user.IsInRole(r));
            }

            if (!string.IsNullOrEmpty(authAttribute.FunctionName))
            {
                MethodInfo mi = t.GetMethod(authAttribute.FunctionName, BindingFlags.Default|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
                if (mi != null && mi.ReturnType == typeof(bool))
                {
                    return (bool) mi.Invoke(entityObject, mi.CreateParameters(httpInformation, serviceProvider));
                }
            }

            return user.Identity.IsAuthenticated;
        }

        private static bool HandleAuthAttribute<T>(this Type t, HttpInformation httpInformation, object entityObject,
            IServiceProvider serviceProvider) where T : AuthAttributeBase
        {
            AuthAttributeBase authAttribute = t.GetCustomAttribute<T>();
            return HandleAuthAttribute(t, authAttribute, httpInformation, entityObject, serviceProvider);
        }
    }
}
