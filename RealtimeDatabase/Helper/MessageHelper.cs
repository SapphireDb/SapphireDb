using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Attributes;
using RealtimeDatabase.Connection;
using RealtimeDatabase.Internal;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Prefilter;
using RealtimeDatabase.Models.Responses;

namespace RealtimeDatabase.Helper
{
    static class MessageHelper
    {
        public static InfoResponse GetInfoResponse(this Type t, RealtimeDbContext db)
        {
            string[] primaryKeys = t.GetPrimaryKeyNames(db);

            InfoResponse infoResponse = new InfoResponse()
            {
                PrimaryKeys = primaryKeys
            };

            GetAttributeData<QueryAuthAttribute>(t, infoResponse, true);
            infoResponse.QueryAuth.Properties = GeneratePropertyInfo<QueryAuthAttribute>(t);

            GetAttributeData<CreateAuthAttribute>(t, infoResponse);
            GetAttributeData<RemoveAuthAttribute>(t, infoResponse);

            GetAttributeData<UpdateAuthAttribute>(t, infoResponse, true);
            infoResponse.UpdateAuth.Properties = GeneratePropertyInfo<UpdateAuthAttribute>(t, pi => pi.GetCustomAttribute<UpdatableAttribute>() != null);

            return infoResponse;
        }

        private static Dictionary<string, AuthInfo> GeneratePropertyInfo<T>(Type t, Func<PropertyInfo, bool> condition = null)
            where T : AuthAttributeBase
        {
            return t.GetProperties()
                .Where(pi => pi.GetCustomAttribute<T>() != null && (condition?.Invoke(pi) ?? true))
                .ToDictionary(
                    pi => pi.Name.ToCamelCase(),
                    pi => {
                        AuthAttributeBase authAttribute = pi.GetCustomAttribute<T>();

                        return new AuthInfo()
                        {
                            Authentication = true,
                            Roles = authAttribute.Roles,
                            FunctionName = authAttribute.FunctionName
                        };
                    }
                );
        }

        private static void GetAttributeData<T>(Type t, InfoResponse infoResponse, bool userPropertyAuth = false) where T : AuthAttributeBase
        {
            AuthAttributeBase authAttribute = t.GetCustomAttribute<T>();
            AuthInfo authInfo = userPropertyAuth ? new PropertyAuthInfo() : new AuthInfo();

            if (authAttribute != null)
            {
                authInfo.Authentication = true;
                authInfo.Roles = authAttribute.Roles;
                authAttribute.FunctionName = authAttribute.FunctionName;
            }
            else
            {
                authInfo.Authentication = false;
            }

            Type attributeType = typeof(T);
            string keyName = attributeType.Name.Remove(attributeType.Name.LastIndexOf("Attribute", StringComparison.Ordinal));
            typeof(InfoResponse).GetProperty(keyName).SetValue(infoResponse, authInfo);
        }

        public static void SendUsersUpdate(IRealtimeAuthContext db, AuthDbContextTypeContainer typeContainer, object usermanager,
            RealtimeConnectionManager connectionManager)
        {
            List<Dictionary<string, object>> users = ModelHelper.GetUsers(db, typeContainer, usermanager).ToList();

            foreach (ConnectionBase connection in connectionManager.connections.Where(wsc => !string.IsNullOrEmpty(wsc.UsersSubscription)))
            {
                _ = connection.Send(new SubscribeUsersResponse()
                {
                    ReferenceId = connection.UsersSubscription,
                    Users = users
                });
            }
        }

        public static void SendRolesUpdate(IRealtimeAuthContext db, RealtimeConnectionManager connectionManager)
        {
            List<Dictionary<string, object>> roles = ModelHelper.GetRoles(db).ToList();

            foreach (ConnectionBase connection in connectionManager.connections.Where(wsc => !string.IsNullOrEmpty(wsc.RolesSubscription)))
            {
                _ = connection.Send(new SubscribeRolesResponse()
                {
                    ReferenceId = connection.RolesSubscription,
                    Roles = roles
                });
            }
        }

        public static ResponseBase GetCollection(RealtimeDbContext db, QueryCommand command,
            HttpContext context, IServiceProvider serviceProvider, out List<object[]> transmittedData)
        {
            KeyValuePair<Type, string> property = db.sets.FirstOrDefault(v => v.Value.ToLowerInvariant() == command.CollectionName.ToLowerInvariant());

            if (property.Key != null)
            {
                IEnumerable<object> collectionSet = db.GetValues(property);

                foreach (IPrefilter prefilter in command.Prefilters.OfType<IPrefilter>())
                {
                    collectionSet = prefilter.Execute(collectionSet);
                }

                List<object> collectionSetList = collectionSet.ToList();

                List<object> result = collectionSetList.Where(cs => property.Key.CanQuery(context, cs, serviceProvider))
                    .Select(cs => cs.GetAuthenticatedQueryModel(context, serviceProvider)).ToList();

                IAfterQueryPrefilter afterQueryPrefilter = command.Prefilters.OfType<IAfterQueryPrefilter>().FirstOrDefault();

                QueryResponse queryResponse = new QueryResponse()
                {
                    Result = afterQueryPrefilter != null ? afterQueryPrefilter.Execute(result) : result,
                    ReferenceId = command.ReferenceId,
                };

                transmittedData = collectionSetList.Select(c => property.Key.GetPrimaryKeyValues(db, c)).ToList();
                return queryResponse;
            }

            transmittedData = new List<object[]>();
            return command.CreateExceptionResponse<QueryResponse>("No set for collection was found.");
        }
    }
}
