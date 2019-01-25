using RealtimeDatabase.Attributes;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Prefilter;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal
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

        public static async Task SendUsersUpdate(IRealtimeAuthContext db, AuthDbContextTypeContainer typeContainer, object usermanager,
            WebsocketConnectionManager connectionManager)
        {
            List<Dictionary<string, object>> users = ModelHelper.GetUsers(db, typeContainer, usermanager).ToList();

            foreach (WebsocketConnection ws in connectionManager.connections.Where(wsc => !string.IsNullOrEmpty(wsc.UsersSubscription)))
            {
                await ws.Send(new SubscribeUsersResponse()
                {
                    ReferenceId = ws.UsersSubscription,
                    Users = users
                });
            }
        }

        public static async Task SendRolesUpdate(IRealtimeAuthContext db, WebsocketConnectionManager connectionManager)
        {
            List<Dictionary<string, object>> roles = ModelHelper.GetRoles(db).ToList();

            foreach (WebsocketConnection ws in connectionManager.connections.Where(wsc => !string.IsNullOrEmpty(wsc.RolesSubscription)))
            {
                await ws.Send(new SubscribeRolesResponse()
                {
                    ReferenceId = ws.RolesSubscription,
                    Roles = roles
                });
            }
        }

        public static async Task<List<object[]>> SendCollection(RealtimeDbContext db, QueryCommand command,
            WebsocketConnection websocketConnection, IServiceProvider serviceProvider)
        {
            KeyValuePair<Type, string> property = db.sets.FirstOrDefault(v => v.Value.ToLowerInvariant() == command.CollectionName.ToLowerInvariant());

            if (property.Key != null)
            {
                IEnumerable<object> collectionSet = db.GetValues(property);

                foreach (IPrefilter prefilter in command.Prefilters)
                {
                    collectionSet = prefilter.Execute(collectionSet);
                }

                List<object> collectionSetList = collectionSet.ToList();

                QueryResponse queryResponse = new QueryResponse()
                {
                    Collection = collectionSetList.Where(cs => property.Key.CanQuery(websocketConnection, cs, serviceProvider))
                                .Select(cs => cs.GetAuthenticatedQueryModel(websocketConnection, serviceProvider)).ToList(),
                    ReferenceId = command.ReferenceId,
                };

                List<object[]> result = collectionSetList.Select(c => property.Key.GetPrimaryKeyValues(db, c)).ToList();
                await websocketConnection.Send(queryResponse);
                return result;
            }

            return new List<object[]>();
        }
    }
}
