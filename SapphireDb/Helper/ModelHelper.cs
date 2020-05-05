using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
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
        public static object[] GetPrimaryKeyValues(this Type type, SapphireDbContext db,
            Dictionary<string, JValue> primaryKeys)
        {
            return type.GetPrimaryKeys(db)
                .Select(p => primaryKeys[p.Name.ToCamelCase()].ToObject(p.ClrType)).ToArray();
        }

        public static object[] GetPrimaryKeyValuesFromJson(this Type type, SapphireDbContext db, JObject jsonObject)
        {
            return type.GetPrimaryKeys(db)
                .Select(p => 
                    jsonObject.GetValue(p.PropertyInfo.Name.ToCamelCase())
                        .ToObject(p.PropertyInfo.PropertyType))
                .ToArray();
        }

        public static DateTime? GetTimestamp(this JObject jsonObject)
        {
            return (DateTime?)jsonObject
                .GetValue("modifiedOn")?
                .ToObject(typeof(DateTime));
        }

        public static bool JsonContainsData(this Type type, SapphireDbContext db, JObject jsonObject)
        {
            bool isOfflineEntity = typeof(SapphireOfflineEntity).IsAssignableFrom(type);
            
            string[] defaultPropertyList = type.GetPrimaryKeys(db)
                .Select(p => p.PropertyInfo.Name.ToCamelCase())
                .Concat(isOfflineEntity ? new [] { "modifiedOn" } : new string[] {})
                .ToArray();
            
            List<JProperty> jsonObjectProperties = jsonObject.Properties().ToList();

            return defaultPropertyList.All(p => jsonObjectProperties.Any(jp => jp.Name == p)) &&
                   jsonObjectProperties.Count > defaultPropertyList.Length;
        }

        public static string[] GetPrimaryKeyNames(this Type type, SapphireDbContext db)
        {
            return type.GetPrimaryKeys(db).Select(p => p.Name.ToCamelCase()).ToArray();
        }

        private static readonly ConcurrentDictionary<Type, IProperty[]> PrimaryKeyDictionary =
            new ConcurrentDictionary<Type, IProperty[]>();

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

        private static readonly ConcurrentDictionary<Type, ModelAttributesInfo> ModelAttributesInfos =
            new ConcurrentDictionary<Type, ModelAttributesInfo>();

        public static ModelAttributesInfo GetModelAttributesInfo(this Type entityType)
        {
            if (ModelAttributesInfos.TryGetValue(entityType, out ModelAttributesInfo authModelInfo))
            {
                return authModelInfo;
            }

            authModelInfo = new ModelAttributesInfo(entityType);
            ModelAttributesInfos.TryAdd(entityType, authModelInfo);

            return authModelInfo;
        }

        private static readonly ConcurrentDictionary<Type, PropertyAttributesInfo[]> PropertyAttributesInfos =
            new ConcurrentDictionary<Type, PropertyAttributesInfo[]>();

        public static PropertyAttributesInfo[] GetPropertyAttributesInfos(this Type entityType)
        {
            if (PropertyAttributesInfos.TryGetValue(entityType, out PropertyAttributesInfo[] propertyInfos))
            {
                return propertyInfos;
            }

            propertyInfos = entityType.GetProperties().Select(p => new PropertyAttributesInfo(p)).ToArray();
            PropertyAttributesInfos.TryAdd(entityType, propertyInfos);

            return propertyInfos;
        }

        public static object SetFields(this Type entityType, object newValues)
        {
            object newEntityObject = Activator.CreateInstance(entityType);

            foreach (PropertyAttributesInfo pi in entityType.GetPropertyAttributesInfos()
                .Where(p => p.NonCreatableAttribute == null))
            {
                pi.PropertyInfo.SetValue(newEntityObject, pi.PropertyInfo.GetValue(newValues));
            }

            return newEntityObject;
        }

        private static List<PropertyAttributesInfo> GetUpdateableProperties(this Type entityType,  object entityObject,
            HttpInformation information, IServiceProvider serviceProvider)
        {
            return entityType.GetPropertyAttributesInfos()
                .Where(info =>
                {
                    if (info.UpdatableAttribute != null ||
                        info.PropertyInfo.DeclaringType.GetModelAttributesInfo().UpdatableAttribute != null)
                    {
                        return info.CanUpdate(information, entityObject, serviceProvider);
                    }

                    return false;
                })
                .ToList();
        }
        
        public static void UpdateFields(this Type entityType, object entityObject, JObject originalValue, JObject updatedProperties,
            HttpInformation information, IServiceProvider serviceProvider)
        {
            List<PropertyAttributesInfo> updateableProperties = entityType.GetUpdateableProperties(entityObject,
                information, serviceProvider);

            foreach (PropertyAttributesInfo pi in updateableProperties)
            {
                JToken updatePropertyToken = updatedProperties.GetValue(pi.PropertyInfo.Name.ToCamelCase());
                JToken originalPropertyToken = originalValue.GetValue(pi.PropertyInfo.Name.ToCamelCase());

                if (updatePropertyToken != null)
                {
                    object updatedPropertyValue = updatePropertyToken.ToObject(pi.PropertyInfo.PropertyType);
                    
                    if (originalPropertyToken != null)
                    {
                        object originalPropertyValue = originalPropertyToken.ToObject(pi.PropertyInfo.PropertyType);

                        if (!originalPropertyValue.Equals(updatedPropertyValue))
                        {
                            pi.PropertyInfo.SetValue(entityObject, updatedPropertyValue);
                        }
                    }
                    else
                    {
                        pi.PropertyInfo.SetValue(entityObject, updatedPropertyValue);
                    }
                }
            }
        }
        
        public static List<Tuple<string, string>> MergeFields(this Type entityType, SapphireOfflineEntity dbObject,
            JObject originalOfflineEntity, JObject updatedProperties, HttpInformation information,
            IServiceProvider serviceProvider)
        {
            List<PropertyAttributesInfo> updateableProperties = entityType.GetUpdateableProperties(dbObject,
                information, serviceProvider);

            List<Tuple<string, string>> mergeErrors = new List<Tuple<string, string>>();

            foreach (PropertyAttributesInfo pi in updateableProperties)
            {
                JToken updatePropertyToken = updatedProperties.GetValue(pi.PropertyInfo.Name.ToCamelCase());

                if (updatePropertyToken != null)
                {
                    object dbPropertyValue = pi.PropertyInfo.GetValue(dbObject);
                    object updatedPropertyValue = updatePropertyToken.ToObject(pi.PropertyInfo.PropertyType);
                    
                    JToken originalPropertyToken = originalOfflineEntity.GetValue(pi.PropertyInfo.Name.ToCamelCase());

                    if (originalPropertyToken != null)
                    {
                        object originalPropertyValue = originalPropertyToken.ToObject(pi.PropertyInfo.PropertyType);

                        if (originalPropertyValue.Equals(updatedPropertyValue))
                        {
                            // Value not updated
                            continue;
                        }
                        
                        if (dbPropertyValue.Equals(originalPropertyValue))
                        {
                            // Value in db equals previous value -> use new value
                            pi.PropertyInfo.SetValue(dbObject, updatedPropertyValue);
                            continue;
                        }
                        
                        if (pi.MergeConflictResolutionModeAttribute != null)
                        {
                            if (pi.MergeConflictResolutionModeAttribute.MergeConflictResolutionMode == MergeConflictResolutionMode.Last)
                            {
                                pi.PropertyInfo.SetValue(dbObject, updatedPropertyValue);
                                mergeErrors.Add(new Tuple<string, string>(pi.PropertyInfo.Name, "value updated"));
                            }
                            else if (pi.MergeConflictResolutionModeAttribute.MergeConflictResolutionMode == MergeConflictResolutionMode.ConflictMarkers &&
                                     dbPropertyValue is string dbPropertyValueString &&
                                     updatedPropertyValue is string updatedPropertyValueString)
                            {
                                string propertyValueConflictMarkers = "<<<<<<< database\n" +
                                                                      $"{dbPropertyValueString}\n" +
                                                                      "=======\n" +
                                                                      $"{updatedPropertyValueString}\n" +
                                                                      ">>>>>>> update";
                                pi.PropertyInfo.SetValue(dbObject, propertyValueConflictMarkers);
                                mergeErrors.Add(new Tuple<string, string>(pi.PropertyInfo.Name, "added conflict markers"));
                            }
                            else
                            {
                                mergeErrors.Add(new Tuple<string, string>(pi.PropertyInfo.Name, "preserved db value"));
                            }
                        }
                        else
                        {
                            mergeErrors.Add(new Tuple<string, string>(pi.PropertyInfo.Name, "no resolution mode active"));
                        }
                    }
                    else
                    {
                        mergeErrors.Add(new Tuple<string, string>(pi.PropertyInfo.Name, "missing information"));
                    }
                }
            }

            return mergeErrors;
        }

        public static IQueryable<object> GetValues(this SapphireDbContext db, KeyValuePair<Type, string> property)
        {
            IQueryable<object> values = (IQueryable<object>) db.GetType().GetProperty(property.Value)?.GetValue(db);
            return values?.AsNoTracking();
        }

        public static void ExecuteHookMethods<T>(this Type modelType, ModelStoreEventAttributeBase.EventType eventType,
            object newValue, HttpInformation httpInformation, IServiceProvider serviceProvider)
            where T : ModelStoreEventAttributeBase
        {
            ModelAttributesInfo modelAttributesInfo = modelType.GetModelAttributesInfo();

            List<T> eventAttributes = null;

            if (typeof(T) == typeof(CreateEventAttribute))
            {
                eventAttributes = modelAttributesInfo.CreateEventAttributes.Cast<T>().ToList();
            }
            else if (typeof(T) == typeof(UpdateEventAttribute))
            {
                eventAttributes = modelAttributesInfo.UpdateEventAttributes.Cast<T>().ToList();
            }
            else if (typeof(T) == typeof(RemoveEventAttribute))
            {
                eventAttributes = modelAttributesInfo.RemoveEventAttributes.Cast<T>().ToList();
            }

            if (eventAttributes == null)
            {
                return;
            }

            foreach (T attribute in eventAttributes)
            {
                if (eventType == ModelStoreEventAttributeBase.EventType.Before)
                {
                    if (attribute.BeforeLambda != null)
                    {
                        attribute.BeforeLambda(newValue, httpInformation);
                    }
                    else if (attribute.BeforeFunction != null)
                    {
                        attribute.BeforeFunction.Invoke(newValue,
                            attribute.BeforeFunction.CreateParameters(httpInformation, serviceProvider));
                    }
                }
                else if (eventType == ModelStoreEventAttributeBase.EventType.BeforeSave)
                {
                    if (attribute.BeforeSaveLambda != null)
                    {
                        attribute.BeforeSaveLambda(newValue, httpInformation);
                    }
                    else if (attribute.BeforeSaveFunction != null)
                    {
                        attribute.BeforeSaveFunction.Invoke(newValue,
                            attribute.BeforeSaveFunction.CreateParameters(httpInformation, serviceProvider));
                    }
                }
                else if (eventType == ModelStoreEventAttributeBase.EventType.After)
                {
                    if (attribute.AfterLambda != null)
                    {
                        attribute.AfterLambda(newValue, httpInformation);
                    }
                    else if (attribute.AfterFunction != null)
                    {
                        attribute.AfterFunction.Invoke(newValue,
                            attribute.AfterFunction.CreateParameters(httpInformation, serviceProvider));
                    }
                }
            }
        }
    }
}