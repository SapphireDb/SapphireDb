using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json.Linq;
using SapphireDb.Attributes;
using SapphireDb.Internal;
using SapphireDb.Internal.Prefilter;
using SapphireDb.Models;

namespace SapphireDb.Helper
{
    static class ModelHelper
    {
        public static object[] GetPrimaryKeyValues(this Type type, SapphireDbContext db, Dictionary<string, JValue> primaryKeys)
        {
            return type.GetPrimaryKeys(db)
                .Select(p => primaryKeys[p.Name.ToCamelCase()].ToObject(p.ClrType)).ToArray();
        }

        public static object[] GetPrimaryKeyValues(this Type type, SapphireDbContext db, object entityObject)
        {
            return type.GetPrimaryKeys(db)
                .Select(p => p.PropertyInfo.GetValue(entityObject)).ToArray();
        }

        public static string[] GetPrimaryKeyNames(this Type type, SapphireDbContext db)
        {
            return type.GetPrimaryKeys(db).Select(p => p.Name.ToCamelCase()).ToArray();
        }

        private static readonly ConcurrentDictionary<Type, IProperty[]> PrimaryKeyDictionary = new ConcurrentDictionary<Type, IProperty[]>();

        public static IProperty[] GetPrimaryKeys(this Type type, SapphireDbContext db)
        {
            if (PrimaryKeyDictionary.TryGetValue(type, out IProperty[] primaryKeys))
            {
                return primaryKeys;
            }

            primaryKeys = db.Model.FindEntityType(type.FullName).FindPrimaryKey().Properties.ToArray();
            PrimaryKeyDictionary.TryAdd(type, primaryKeys);

            return primaryKeys;
        }

        private static readonly ConcurrentDictionary<Type, AuthModelInfo> AuthModelInfosDictionary = new ConcurrentDictionary<Type, AuthModelInfo>();

        public static AuthModelInfo GetAuthModelInfos(this Type entityType)
        {
            if (AuthModelInfosDictionary.TryGetValue(entityType, out AuthModelInfo authModelInfo))
            {
                return authModelInfo;
            }

            authModelInfo = new AuthModelInfo(entityType);
            AuthModelInfosDictionary.TryAdd(entityType, authModelInfo);

            return authModelInfo;
        }
        
        private static readonly ConcurrentDictionary<Type, AuthPropertyInfo[]> PropertyInfosDictionary = new ConcurrentDictionary<Type, AuthPropertyInfo[]>();

        public static AuthPropertyInfo[] GetAuthPropertyInfos(this Type entityType)
        {
            if (PropertyInfosDictionary.TryGetValue(entityType, out AuthPropertyInfo[] propertyInfos))
            {
                return propertyInfos;
            }

            propertyInfos = entityType.GetProperties().Select(p => new AuthPropertyInfo(p)).ToArray();
            PropertyInfosDictionary.TryAdd(entityType, propertyInfos);

            return propertyInfos;
        }

        public static object SetFields(this Type entityType, object newValues, SapphireDbContext db)
        {
            object newEntityObject = Activator.CreateInstance(entityType);
            string[] primaryKeys = entityType.GetPrimaryKeyNames(db);

            foreach (AuthPropertyInfo pi in entityType.GetAuthPropertyInfos())
            {
                if (pi.NonCreatableAttribute == null && !primaryKeys.Contains(pi.PropertyInfo.Name.ToCamelCase()))
                {
                    pi.PropertyInfo.SetValue(newEntityObject, pi.PropertyInfo.GetValue(newValues));
                }
            }

            return newEntityObject;
        }
        
        public static void UpdateFields(this Type entityType, object entityObject, object newValues,
            SapphireDbContext db, HttpInformation information, IServiceProvider serviceProvider)
        {
            List<AuthPropertyInfo> updatableProperties = entityType.GetAuthPropertyInfos()
                .Where(info =>
                {
                    if (info.UpdatableAttribute != null ||
                        info.PropertyInfo.DeclaringType?.GetCustomAttribute<UpdatableAttribute>(false) != null)
                    {
                        return info.CanUpdate(information, entityObject, serviceProvider);
                    }

                    return false;
                })
                .ToList();
            
            foreach (AuthPropertyInfo pi in updatableProperties)
            {
                pi.PropertyInfo.SetValue(entityObject, pi.PropertyInfo.GetValue(newValues));
            }
        }

        public static IQueryable<object> GetValues(this SapphireDbContext db, KeyValuePair<Type, string> property, IServiceProvider serviceProvider, HttpInformation httpInformation)
        {
            IQueryable<object> values = (IQueryable<object>)db.GetType().GetProperty(property.Value)?.GetValue(db);

            QueryFunctionAttribute queryFunctionAttribute = property.Key.GetCustomAttribute<QueryFunctionAttribute>(false);
            if (queryFunctionAttribute != null)
            {
                var queryFunctionInfo = property.Key.GetMethod(queryFunctionAttribute.Function, BindingFlags.Default|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static);

                if (queryFunctionInfo != null)
                {
                    object[] methodParameters = queryFunctionInfo.CreateParameters(httpInformation, serviceProvider);
                    Expression queryFunctionExpression = (Expression)queryFunctionInfo.Invoke(null, methodParameters);

                    MethodInfo whereMethodInfo = typeof(Queryable).GetMethods(BindingFlags.Static|BindingFlags.Public).FirstOrDefault(mi => mi.Name == "Where");
                    whereMethodInfo = whereMethodInfo?.MakeGenericMethod(property.Key);

                    values = (IQueryable<object>)whereMethodInfo?.Invoke(values, new object[] {values, queryFunctionExpression});
                }
            }

            return values;
        }

        public static IQueryable<object> GetCollectionValues(this SapphireDbContext db, IServiceProvider serviceProvider, HttpInformation information, KeyValuePair<Type, string> property, List<IPrefilterBase> prefilters)
        {
            IQueryable<object> collectionSet = db.GetValues(property, serviceProvider, information);

            foreach (IPrefilter prefilter in prefilters.OfType<IPrefilter>())
            {
                prefilter.Initialize(property.Key);
                collectionSet = prefilter.Execute(collectionSet);
            }

            return collectionSet;
        }

        public static void ExecuteHookMethods<T>(this Type modelType, Func<ModelStoreEventAttributeBase, string> methodSelector,
            object newValue, HttpInformation httpInformation, IServiceProvider serviceProvider) where T : ModelStoreEventAttributeBase
        {
            List<T> attributes = modelType.GetCustomAttributes<T>().ToList();

            attributes.ForEach(attribute =>
            {
                string methodName = methodSelector(attribute);

                if (!String.IsNullOrEmpty(methodName))
                {
                    MethodInfo methodInfo = modelType.GetMethod(methodName,
                        BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (methodInfo != null && methodInfo.ReturnType == typeof(void))
                    {
                        methodInfo.Invoke(newValue, methodInfo.CreateParameters(httpInformation, serviceProvider));
                    }
                }
            });
        }
    }
}
