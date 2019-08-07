namespace RealtimeDatabase.Internal.CommandHandler
{
    class CommandHandlerBase
    {
        protected readonly DbContextAccesor contextAccessor;

        public CommandHandlerBase(DbContextAccesor contextAccessor)
        {
            this.contextAccessor = contextAccessor;
        }

        protected RealtimeDbContext GetContext(string contextName)
        {
            return contextAccessor.GetContext(contextName);
        }
    }
}
