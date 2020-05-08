namespace SapphireDb.Command
{
    public class CollectionCommandBase : CommandBase
    {
        private string collectionName;
        private string contextName = "default";

        public string CollectionName
        {
            get
            {
                return collectionName;
            }
            set
            {
                string[] collectionNameParts = value.Split('.');

                if (collectionNameParts.Length == 1)
                {
                    collectionName = collectionNameParts[0];
                }
                else if (collectionNameParts.Length == 2)
                {
                    contextName = collectionNameParts[0];
                    collectionName = collectionNameParts[1];
                }
                else
                {
                    collectionName = value;
                }
                
            }
        }

        public string ContextName
        {
            get { return contextName; }
        }
    }
}
