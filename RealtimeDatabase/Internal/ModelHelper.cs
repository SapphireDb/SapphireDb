using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json.Linq;
using RealtimeDatabase.Attributes;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;

namespace RealtimeDatabase.Internal
{
    static class ModelHelper
    {
        public static object[] GetPrimaryKeyValues(this Type type, RealtimeDbContext db, Dictionary<string, JValue> primaryKeys)
        {
            return type.GetPrimaryKeys(db)
                .Select(p => primaryKeys[p.Name.ToCamelCase()].ToObject(p.ClrType)).ToArray();
        }

        public static object[] GetPrimaryKeyValues(this Type type, RealtimeDbContext db, object entityObject)
        {
            return type.GetPrimaryKeys(db)
                .Select(p => p.PropertyInfo.GetValue(entityObject)).ToArray();
        }

        public static string[] GetPrimaryKeyNames(this Type type, RealtimeDbContext db)
        {
            return type.GetPrimaryKeys(db).Select(p => p.Name.ToCamelCase()).ToArray();
        }

        public static IProperty[] GetPrimaryKeys(this Type type, RealtimeDbContext db)
        {
            return db.Model.FindEntityType(type.FullName).FindPrimaryKey().Properties.ToArray();
        }

        public static void UpdateFields(this Type entityType, object entityObject, object newValues, RealtimeDbContext db, WebsocketConnection websocketConnection)
        {
            string[] primaryKeys = entityType.GetPrimaryKeyNames(db);

            if (entityType.GetCustomAttribute<UpdatableAttribute>() != null)
            {
                foreach (PropertyInfo pi in entityType.GetProperties())
                {
                    if (!primaryKeys.Contains(pi.Name.ToCamelCase()))
                    {
                        if (pi.CanUpdate(websocketConnection, entityObject))
                        {
                            pi.SetValue(entityObject, pi.GetValue(newValues));
                        }
                    }
                }
            }
            else
            {
                foreach (PropertyInfo pi in entityType.GetProperties())
                {
                    if (pi.GetCustomAttribute<UpdatableAttribute>() != null)
                    {
                        if (!primaryKeys.Contains(pi.Name.ToCamelCase()))
                        {
                            if (pi.CanUpdate(websocketConnection, entityObject))
                            {
                                pi.SetValue(entityObject, pi.GetValue(newValues));
                            }
                        }
                    }
                }
            }
        }

        public static InfoResponse GetInfoResponse(this Type t, RealtimeDbContext db)
        {
            string[] primaryKeys = t.GetPrimaryKeyNames(db);

            InfoResponse infoResponse = new InfoResponse()
            {
                PrimaryKeys = primaryKeys
            };

            QueryAuthAttribute queryAuthAttribute = t.GetCustomAttribute<QueryAuthAttribute>();

            Dictionary<string, AuthInfo> propertiesQueryAuthInfo = t.GetProperties()
                .Where(pi => pi.GetCustomAttribute<QueryAuthAttribute>() != null)
                .ToDictionary(
                    pi => pi.Name.ToCamelCase(), 
                    pi => new AuthInfo()
                    {
                        Authentication = true,
                        Roles = pi.GetCustomAttribute<QueryAuthAttribute>().Roles
                    }
                );

            if (queryAuthAttribute != null)
            {
                infoResponse.QueryAuth = new PropertyAuthInfo()
                {
                    Authentication = true,
                    Roles = queryAuthAttribute.Roles,
                    Properties = propertiesQueryAuthInfo
                };
            }
            else
            {
                infoResponse.QueryAuth = new PropertyAuthInfo()
                {
                    Authentication = false,
                    Properties = propertiesQueryAuthInfo
                };
            }

            CreateAuthAttribute createAuthAttribute= t.GetCustomAttribute<CreateAuthAttribute>();

            if (createAuthAttribute != null)
            {
                infoResponse.CreateAuth = new AuthInfo()
                {
                    Authentication = true,
                    Roles = createAuthAttribute.Roles
                };
            }
            else
            {
                infoResponse.CreateAuth = new AuthInfo()
                {
                    Authentication = false
                };
            }

            RemoveAuthAttribute removeAuthAttribute = t.GetCustomAttribute<RemoveAuthAttribute>();

            if (removeAuthAttribute != null)
            {
                infoResponse.RemoveAuth = new AuthInfo()
                {
                    Authentication = true,
                    Roles = removeAuthAttribute.Roles
                };
            }
            else
            {
                infoResponse.RemoveAuth = new AuthInfo()
                {
                    Authentication = false
                };
            }

            UpdateAuthAttribute updateAuthAttribute = t.GetCustomAttribute<UpdateAuthAttribute>();

            Dictionary<string, AuthInfo> propertiesUpdateAuthInfo = t.GetProperties()
                .Where(pi => pi.GetCustomAttribute<UpdatableAttribute>() != null && pi.GetCustomAttribute<UpdateAuthAttribute>() != null)
                .ToDictionary(
                    pi => pi.Name.ToCamelCase(),
                    pi => new AuthInfo()
                    {
                        Authentication = true,
                        Roles = pi.GetCustomAttribute<UpdateAuthAttribute>().Roles
                    }
                );

            if (updateAuthAttribute != null)
            {
                infoResponse.UpdateAuth = new PropertyAuthInfo()
                {
                    Authentication = true,
                    Roles = updateAuthAttribute.Roles,
                    Properties = propertiesUpdateAuthInfo
                };
            }
            else
            {
                infoResponse.UpdateAuth = new PropertyAuthInfo()
                {
                    Authentication = false,
                    Properties = propertiesUpdateAuthInfo
                };
            }

            return infoResponse;
        }

        public static Dictionary<string, object> GenerateUserData(IdentityUser identityUser)
        {
            Dictionary<string, object> userData = new Dictionary<string, object>();
            Type t = identityUser.GetType();

            IEnumerable<PropertyInfo> properties =
                t.GetProperties().Where(p => p.GetCustomAttribute<AuthUserInformationAttribute>() != null
                || p.Name == "Id" || p.Name == "UserName" || p.Name == "Email");

            foreach (PropertyInfo property in properties)
            {
                if (property.Name != "Roles")
                {
                    userData[property.Name] = property.GetValue(identityUser);
                }
                else
                {
                    userData["_Roles"] = property.GetValue(identityUser);
                }
            }

            return userData;
        }
    }
}
