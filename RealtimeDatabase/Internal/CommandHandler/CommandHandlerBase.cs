namespace RealtimeDatabase.Internal.CommandHandler
{
    class CommandHandlerBase
    {
        protected readonly DbContextAccesor contextAccesor;

        public CommandHandlerBase(DbContextAccesor contextAccesor)
        {
            this.contextAccesor = contextAccesor;
        }

        protected RealtimeDbContext GetContext()
        {
            return contextAccesor.GetContext();
        }
    }
}
