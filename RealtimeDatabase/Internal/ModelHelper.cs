using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json.Linq;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using static RealtimeDatabase.RealtimeAuthorizeAttribute;

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

        public static void UpdateFields(Type entityType, object entityObject, object newValues, RealtimeDbContext db)
        {
            if (entityType.GetCustomAttribute<UpdatableAttribute>() != null)
            {
                string[] primaryKeys = entityType.GetPrimaryKeyNames(db);

                foreach (PropertyInfo pi in entityType.GetProperties())
                {
                    if (!primaryKeys.Contains(pi.Name.ToCamelCase()))
                    {
                        pi.SetValue(entityObject, pi.GetValue(newValues));
                    }
                }
            }
            else
            {
                foreach (PropertyInfo pi in entityType.GetProperties())
                {
                    if (pi.GetCustomAttribute<UpdatableAttribute>() != null)
                    {
                        pi.SetValue(entityObject, pi.GetValue(newValues));
                    }
                }
            }
        }

        public static bool IsAuthorized(this Type t, WebsocketConnection connection, OperationType operationType)
        {
            RealtimeAuthorizeAttribute authorizeAttribute = t.GetCustomAttribute<RealtimeAuthorizeAttribute>();

            if (authorizeAttribute == null)
            {
                return true;
            }

            ClaimsPrincipal user = connection.HttpContext.User;

            if (authorizeAttribute.RolesRead == null || authorizeAttribute.RolesWrite == null || authorizeAttribute.RolesDelete == null)
            {
                return user.Identity.IsAuthenticated;
            }

            if (user.Identity.IsAuthenticated)
            {
                if (operationType == OperationType.Read)
                {
                    if (authorizeAttribute.RolesRead.Any(r => user.IsInRole(r)))
                    {
                        return true;
                    }
                }
                else if (operationType == OperationType.Write)
                {
                    if (authorizeAttribute.RolesWrite.Any(r => user.IsInRole(r)))
                    {
                        return true;
                    }
                }
                else
                {
                    if (authorizeAttribute.RolesDelete.Any(r => user.IsInRole(r)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
