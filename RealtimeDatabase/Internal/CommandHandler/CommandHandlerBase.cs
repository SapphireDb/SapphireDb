namespace RealtimeDatabase.Internal.CommandHandler
{
    class CommandHandlerBase
    {
        protected readonly DbContextAccesor contextAccesor;

        public CommandHandlerBase(DbContextAccesor _contextAccesor)
        {
            contextAccesor = _contextAccesor;
        }

        protected RealtimeDbContext GetContext()
        {
            return contextAccesor.GetContext();
        }
    }
}
