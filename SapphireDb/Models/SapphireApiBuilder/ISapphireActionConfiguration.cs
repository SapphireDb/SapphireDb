namespace SapphireDb.Models.SapphireApiBuilder
{
    public interface ISapphireActionConfiguration
    {
    }
    
    public interface ISapphireActionConfiguration<T> : ISapphireActionConfiguration
    {
        public void Configure(SapphireActionHandlerBuilder<T> actionHandlerBuilder);
    }
}