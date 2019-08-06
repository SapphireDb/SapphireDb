namespace RealtimeDatabase.Internal.CommandHandler
{
    class CommandHandlerBase
    {
        protected readonly DbContextAccesor contextAccessor;

        public CommandHandlerBase(DbContextAccesor contextAccessor)
        {
            this.contextAccessor = contextAccessor;
        }

        protected RealtimeDbContext GetContext(string contextName = "Default")
        {
            return contextAccessor.GetContext(contextName);
        }
    }
}
