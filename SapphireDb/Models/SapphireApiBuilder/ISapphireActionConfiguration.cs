using SapphireDb.Actions;

namespace SapphireDb.Models.SapphireApiBuilder
{
    public interface ISapphireActionConfiguration
    {
    }
    
    public interface ISapphireActionConfiguration<T> : ISapphireActionConfiguration
        where T : ActionHandlerBase
    {
        public void Configure(SapphireActionHandlerBuilder<T> actionHandlerBuilder);
    }
}