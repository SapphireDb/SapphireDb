using Newtonsoft.Json.Linq;
using SapphireDb.Command.Query;
using SapphireDb.Models.Exceptions;

namespace SapphireDb.Command.QueryQuery
{
    public class QueryQueryCommand : CommandBase, IQueryCommand
    {
        private string collectionName;
        private string contextName = "default";
        private string queryName;
        
        public string QueryName
        {
            get => queryName;
            set
            {
                string[] collectionNameParts = value.Split('.');

                if (collectionNameParts.Length == 3)
                {
                    queryName = collectionNameParts[2];
                    collectionName = collectionNameParts[1];
                    contextName = collectionNameParts[0];
                }
                else if (collectionNameParts.Length == 2)
                {
                    queryName = collectionNameParts[1];
                    collectionName = collectionNameParts[0];
                }
                else
                {
                    throw new WrongQueryFormatException(value);
                }
                
            }
        }

        public string CollectionName => collectionName;
        
        public string ContextName => contextName;

        public JToken[] Parameters { get; set; }
    }
}