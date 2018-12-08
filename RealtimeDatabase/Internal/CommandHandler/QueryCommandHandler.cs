using RealtimeDatabase.Models;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Prefilter;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class QueryCommandHandler : CommandHandlerBase, ICommandHandler<QueryCommand>
    {
        public QueryCommandHandler(DbContextAccesor dbContextAccesor)
            : base(dbContextAccesor)
        {

        }

        public async Task Handle(WebsocketConnection websocketConnection, QueryCommand command)
        {
            await HandleInner(websocketConnection, command);
        }

        public async Task<List<object[]>> HandleInner(WebsocketConnection websocketConnection, QueryCommand command)
        {
            RealtimeDbContext db = GetContext();

            KeyValuePair<Type, string> property = db.sets.FirstOrDefault(v => v.Value.ToLowerInvariant() == command.CollectionName.ToLowerInvariant());

            if (property.Key != null)
            {
                if (!property.Key.CanQuery(websocketConnection))
                {
                    await SendMessage(websocketConnection, new QueryResponse()
                    {
                        ReferenceId = command.ReferenceId,
                        Collection = new List<object>(),
                        Error = new Exception("The user is not authorized for this action.")
                    });

                    return new List<object[]>();
                }

                IEnumerable<object> collectionSet = (IEnumerable<object>)db.GetType().GetProperty(property.Value).GetValue(db);

                foreach (IPrefilter prefilter in command.Prefilters)
                {
                    collectionSet = prefilter.Execute(collectionSet);
                }

                QueryResponse queryResponse = new QueryResponse()
                {
                    Collection = collectionSet.Select(cs => cs.GetAuthenticatedQueryModel(websocketConnection)),
                    ReferenceId = command.ReferenceId,
                };

                await SendMessage(websocketConnection, queryResponse);

                return collectionSet.Select(c => property.Key.GetPrimaryKeyValues(db, c)).ToList();
            }

            return new List<object[]>();
        }
    }
}
