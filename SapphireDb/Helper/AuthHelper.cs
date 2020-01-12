using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SapphireDb.Actions;
using SapphireDb.Attributes;
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

        public static bool CanQuery(this Type t, HttpInformation httpInformation, IServiceProvider serviceProvider,
            object entityObject = null)
        {
            return t.CallHandleAuthAttribute(SapphireAuthResource.OperationTypeEnum.Query, httpInformation,
                entityObject, serviceProvider);
        }
        
        public static bool CanQueryEntry(this Type t, HttpInformation httpInformation, IServiceProvider serviceProvider,
            object entityObject = null)
        {
            return HandleAuthAttributes(t.GetModelAttributesInfo().QueryEntryAuthAttributes, httpInformation,
                SapphireAuthResource.OperationTypeEnum.Query, entityObject, serviceProvider);
        }

        public static bool CanQuery(this PropertyAttributesInfo pi, HttpInformation httpInformation, object entityObject,
            IServiceProvider serviceProvider)
        {
            return HandleAuthAttributes(pi.QueryAuthAttributes, httpInformation,
                SapphireAuthResource.OperationTypeEnum.Query, entityObject, serviceProvider);
        }

        public static bool CanCreate(this Type t, HttpInformation httpInformation, object entityObject,
            IServiceProvider serviceProvider)
        {
            return t.CallHandleAuthAttribute(SapphireAuthResource.OperationTypeEnum.Create, httpInformation,
                entityObject, serviceProvider);
        }

        public static bool CanRemove(this Type t, HttpInformation httpInformation, object entityObject,
            IServiceProvider serviceProvider)
        {
            return t.CallHandleAuthAttribute(SapphireAuthResource.OperationTypeEnum.Remove, httpInformation,
                entityObject, serviceProvider);
        }

        public static bool CanUpdate(this Type t, HttpInformation httpInformation, object entityObject,
            IServiceProvider serviceProvider)
        {
            return t.CallHandleAuthAttribute(SapphireAuthResource.OperationTypeEnum.Update, httpInformation,
                entityObject, serviceProvider);
        }

        public static bool CanUpdate(this PropertyAttributesInfo pi, HttpInformation httpInformation, object entityObject,
            IServiceProvider serviceProvider)
        {
            return HandleAuthAttributes(pi.UpdateAuthAttributes, httpInformation,
                SapphireAuthResource.OperationTypeEnum.Update, entityObject, serviceProvider);
        }

        public static object GetAuthenticatedQueryModel(this object model, HttpInformation httpInformation,
            IServiceProvider serviceProvider)
        {
            PropertyAttributesInfo[] propertyInfos = model.GetType().GetPropertyAttributesInfos();

            if (propertyInfos.All(pi => !pi.QueryAuthAttributes.Any()))
            {
                return model;
            }

            Dictionary<string, object> value = new Dictionary<string, object>();

            foreach (PropertyAttributesInfo pi in propertyInfos)
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
            return HandleAuthAttributes(type.GetActionHandlerAttributesInfo().ActionAuthAttributes, httpInformation,
                SapphireAuthResource.OperationTypeEnum.Execute, actionHandler, serviceProvider);
        }

        public static bool CanExecuteAction(this MethodInfo methodInfo, HttpInformation httpInformation,
            ActionHandlerBase actionHandler, IServiceProvider serviceProvider)
        {
            return HandleAuthAttributes(methodInfo.GetActionAttributesInfo().ActionAuthAttributes, httpInformation,
                SapphireAuthResource.OperationTypeEnum.Execute, actionHandler, serviceProvider);
        }

        private static bool HandleAuthAttributes<T>(List<T> authAttributes,
            HttpInformation httpInformation,
            SapphireAuthResource.OperationTypeEnum operationTypeEnum, object entityObject,
            IServiceProvider serviceProvider) where T : AuthAttributeBase
        {
            if (!authAttributes.Any())
            {
                return true;
            }

            return authAttributes.Any(authAttribute => HandleAuthAttribute(authAttribute, httpInformation,
                operationTypeEnum, entityObject, serviceProvider));
        }

        private static bool HandleAuthAttribute(AuthAttributeBase authAttribute,
            HttpInformation httpInformation,
            SapphireAuthResource.OperationTypeEnum operationTypeEnum, object entityObject,
            IServiceProvider serviceProvider)
        {
            ClaimsPrincipal user = httpInformation.User;

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

            if (authAttribute.FunctionInfo != null)
            {
                return (bool) authAttribute.FunctionInfo.Invoke(entityObject,
                    authAttribute.FunctionInfo.CreateParameters(httpInformation, serviceProvider));
            }

            return user.Identity.IsAuthenticated;
        }

        private static bool CallHandleAuthAttribute(this Type t,
            SapphireAuthResource.OperationTypeEnum operationTypeEnum,
            HttpInformation httpInformation, object entityObject, IServiceProvider serviceProvider)
        {
            ModelAttributesInfo modelAttributesInfo = t.GetModelAttributesInfo();
            
            switch (operationTypeEnum)
            {
                case SapphireAuthResource.OperationTypeEnum.Create:
                    return HandleAuthAttributes(modelAttributesInfo.CreateAuthAttributes,
                        httpInformation, operationTypeEnum, entityObject, serviceProvider);
                case SapphireAuthResource.OperationTypeEnum.Remove:
                    return HandleAuthAttributes(modelAttributesInfo.RemoveAuthAttributes,
                        httpInformation, operationTypeEnum, entityObject, serviceProvider);
                case SapphireAuthResource.OperationTypeEnum.Update:
                    return HandleAuthAttributes(modelAttributesInfo.UpdateAuthAttributes,
                        httpInformation, operationTypeEnum, entityObject, serviceProvider);
                default:
                    return HandleAuthAttributes(modelAttributesInfo.QueryAuthAttributes,
                        httpInformation, operationTypeEnum, entityObject, serviceProvider);
            }

            
        }
    }
}