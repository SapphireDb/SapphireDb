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
using System.Text;
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

            CreateAuthAttribute createAuthAttribute = t.GetCustomAttribute<CreateAuthAttribute>();

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

        public static async Task SendUsersUpdate(IRealtimeAuthContext db, AuthDbContextTypeContainer typeContainer, object usermanager,
            WebsocketConnectionManager connectionManager)
        {
            List<Dictionary<string, object>> users = ModelHelper.GetUsers(db, typeContainer, usermanager).ToList();

            foreach (WebsocketConnection ws in connectionManager.connections.Where(wsc => !String.IsNullOrEmpty(wsc.UsersSubscription)))
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

            foreach (WebsocketConnection ws in connectionManager.connections.Where(wsc => !String.IsNullOrEmpty(wsc.RolesSubscription)))
            {
                await ws.Send(new SubscribeRolesResponse()
                {
                    ReferenceId = ws.RolesSubscription,
                    Roles = roles
                });
            }
        }

        public static async Task<List<object[]>> SendCollection(RealtimeDbContext db, QueryCommand command, WebsocketConnection websocketConnection)
        {
            KeyValuePair<Type, string> property = db.sets.FirstOrDefault(v => v.Value.ToLowerInvariant() == command.CollectionName.ToLowerInvariant());

            if (property.Key != null)
            {
                IEnumerable<object> collectionSet = (IEnumerable<object>)db.GetType().GetProperty(property.Value).GetValue(db);
                collectionSet = collectionSet.ToList();


                foreach (IPrefilter prefilter in command.Prefilters)
                {
                    collectionSet = prefilter.Execute(collectionSet);
                }

                collectionSet = collectionSet.ToList();

                QueryResponse queryResponse = new QueryResponse()
                {
                    Collection = collectionSet.Where(cs => property.Key.CanQuery(websocketConnection, cs))
                                .Select(cs => cs.GetAuthenticatedQueryModel(websocketConnection)).ToList(),
                    ReferenceId = command.ReferenceId,
                };

                List<object[]> result = collectionSet.Select(c => property.Key.GetPrimaryKeyValues(db, c)).ToList();
                await websocketConnection.Send(queryResponse);
                return result;
            }

            return new List<object[]>();
        }
    }
}
