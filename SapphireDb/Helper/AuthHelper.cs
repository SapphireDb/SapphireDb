using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using SapphireDb.Actions;
using SapphireDb.Attributes;
using SapphireDb.Connection;
using SapphireDb.Internal;
using SapphireDb.Models;

namespace SapphireDb.Helper
{
    static class AuthHelper
    {
        public static bool CheckApiAuth(string key, string secret, SapphireDatabaseOptions options)
        {
            return !options.ApiConfigurations.Any() || options.ApiConfigurations.Any((config) =>
                       config.Key == key && config.Secret == secret.ComputeHash());
        }

        public static bool CanQuery(this Type t, IConnectionInformation connectionInformation, IServiceProvider serviceProvider)
        {
            return t.CallHandleAuthAttribute(SapphireAuthResource.OperationTypeEnum.Query, connectionInformation,
                null, serviceProvider);
        }
        
        public static bool CanQueryEntry(this Type t, IConnectionInformation connectionInformation, IServiceProvider serviceProvider,
            object entityObject = null)
        {
            return HandleAuthAttributes(t.GetModelAttributesInfo().QueryEntryAuthAttributes, connectionInformation,
                SapphireAuthResource.OperationTypeEnum.Query, entityObject, serviceProvider);
        }

        public static bool CanQuery(this PropertyAttributesInfo pi, IConnectionInformation connectionInformation, object entityObject,
            IServiceProvider serviceProvider, bool propertyWhitelist, bool propertyBlacklist)
        {
            if (propertyWhitelist)
            {
                if (pi.ExposeAttribute == null)
                {
                    return false;
                }
            }

            if (propertyBlacklist)
            {
                if (pi.ConcealAttribute != null)
                {
                    return false;
                }
            }
            
            return HandleAuthAttributes(pi.QueryAuthAttributes, connectionInformation,
                SapphireAuthResource.OperationTypeEnum.Query, entityObject, serviceProvider);
        }

        public static bool CanCreate(this Type t, IConnectionInformation connectionInformation, object entityObject,
            IServiceProvider serviceProvider)
        {
            return t.CallHandleAuthAttribute(SapphireAuthResource.OperationTypeEnum.Create, connectionInformation,
                entityObject, serviceProvider);
        }

        public static bool CanDelete(this Type t, IConnectionInformation connectionInformation, object entityObject,
            IServiceProvider serviceProvider)
        {
            return t.CallHandleAuthAttribute(SapphireAuthResource.OperationTypeEnum.Delete, connectionInformation,
                entityObject, serviceProvider);
        }

        public static bool CanUpdate(this Type t, IConnectionInformation connectionInformation, object entityObject,
            IServiceProvider serviceProvider, JObject newValue)
        {
            return t.CallHandleAuthAttribute(SapphireAuthResource.OperationTypeEnum.Update, connectionInformation,
                entityObject, serviceProvider, newValue);
        }

        public static bool CanUpdate(this PropertyAttributesInfo pi, IConnectionInformation connectionInformation, object entityObject,
            IServiceProvider serviceProvider, JObject newValue)
        {
            return HandleAuthAttributes(pi.UpdateAuthAttributes, connectionInformation,
                SapphireAuthResource.OperationTypeEnum.Update, entityObject, serviceProvider, newValue);
        }

        public static object GetAuthenticatedQueryModel(this object model, IConnectionInformation connectionInformation,
            IServiceProvider serviceProvider)
        {
            PropertyAttributesInfo[] propertyInfos = model.GetType().GetPropertyAttributesInfos();

            bool usePropertyWhitelist = propertyInfos.Any(pi => pi.ExposeAttribute != null); 
            bool usePropertyBlacklist = propertyInfos.Any(pi => pi.ConcealAttribute != null);
            
            if (propertyInfos.All(pi => !pi.QueryAuthAttributes.Any()) && !usePropertyBlacklist && !usePropertyWhitelist)
            {
                return model;
            }

            Dictionary<string, object> value = new Dictionary<string, object>();

            foreach (PropertyAttributesInfo pi in propertyInfos)
            {
                if (pi.CanQuery(connectionInformation, model, serviceProvider, usePropertyWhitelist, usePropertyBlacklist))
                {
                    value.Add(pi.PropertyInfo.Name.ToCamelCase(), pi.PropertyInfo.GetValue(model));
                }
            }

            return value;
        }

        public static bool CanExecuteAction(this Type type, IConnectionInformation connectionInformation,
            ActionHandlerBase actionHandler, IServiceProvider serviceProvider)
        {
            return HandleAuthAttributes(type.GetActionHandlerAttributesInfo().ActionAuthAttributes, connectionInformation,
                SapphireAuthResource.OperationTypeEnum.Execute, actionHandler, serviceProvider);
        }

        public static bool CanExecuteAction(this MethodInfo methodInfo, IConnectionInformation connectionInformation,
            ActionHandlerBase actionHandler, IServiceProvider serviceProvider)
        {
            return HandleAuthAttributes(methodInfo.GetActionAttributesInfo().ActionAuthAttributes, connectionInformation,
                SapphireAuthResource.OperationTypeEnum.Execute, actionHandler, serviceProvider);
        }

        private static bool HandleAuthAttributes<T>(List<T> authAttributes,
            IConnectionInformation connectionInformation,
            SapphireAuthResource.OperationTypeEnum operationTypeEnum, object entityObject,
            IServiceProvider serviceProvider, JObject newValue = null) where T : AuthAttributeBase
        {
            if (!authAttributes.Any())
            {
                return true;
            }

            return authAttributes.Any(authAttribute => HandleAuthAttribute(authAttribute, connectionInformation,
                operationTypeEnum, entityObject, serviceProvider, newValue));
        }

        private static bool HandleAuthAttribute(AuthAttributeBase authAttribute,
            IConnectionInformation connectionInformation,
            SapphireAuthResource.OperationTypeEnum operationTypeEnum, object entityObject,
            IServiceProvider serviceProvider, JObject newValue)
        {
            ClaimsPrincipal user = connectionInformation.User;

            if (authAttribute.Policies.Any())
            {
                SapphireAuthResource authResource = new SapphireAuthResource()
                {
                    OperationType = operationTypeEnum,
                    RequestedResource = entityObject
                };

                IAuthorizationService authorizationService = serviceProvider.GetService<IAuthorizationService>();

                foreach (string policy in authAttribute.Policies)
                {
                    if (!authorizationService.AuthorizeAsync(user, authResource, policy).Result.Succeeded)
                    {
                        return false;
                    }
                }
            }

            if (authAttribute.FunctionLambda != null)
            {
                return authAttribute.FunctionLambda(connectionInformation, entityObject);
            }
            
            if (authAttribute.FunctionInfo != null)
            {
                return (bool) authAttribute.FunctionInfo.Invoke(entityObject,
                    authAttribute.FunctionInfo.CreateParameters(connectionInformation, serviceProvider, newValue));
            }

            return user.Identity.IsAuthenticated;
        }

        private static bool CallHandleAuthAttribute(this Type t,
            SapphireAuthResource.OperationTypeEnum operationTypeEnum, IConnectionInformation connectionInformation,
            object entityObject, IServiceProvider serviceProvider, JObject newValue = null)
        {
            ModelAttributesInfo modelAttributesInfo = t.GetModelAttributesInfo();
            
            switch (operationTypeEnum)
            {
                case SapphireAuthResource.OperationTypeEnum.Create:
                    return HandleAuthAttributes(modelAttributesInfo.CreateAuthAttributes,
                        connectionInformation, operationTypeEnum, entityObject, serviceProvider, newValue);
                case SapphireAuthResource.OperationTypeEnum.Delete:
                    return HandleAuthAttributes(modelAttributesInfo.DeleteAuthAttributes,
                        connectionInformation, operationTypeEnum, entityObject, serviceProvider, newValue);
                case SapphireAuthResource.OperationTypeEnum.Update:
                    return HandleAuthAttributes(modelAttributesInfo.UpdateAuthAttributes,
                        connectionInformation, operationTypeEnum, entityObject, serviceProvider, newValue);
                default:
                    return HandleAuthAttributes(modelAttributesInfo.QueryAuthAttributes,
                        connectionInformation, operationTypeEnum, entityObject, serviceProvider, newValue);
            }
        }
    }
}