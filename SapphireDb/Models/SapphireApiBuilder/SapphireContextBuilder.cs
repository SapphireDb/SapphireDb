using Microsoft.Extensions.DependencyInjection;

namespace SapphireDb.Models.SapphireApiBuilder
{
    public class SapphireContextBuilder
    {
        private readonly IServiceCollection serviceCollection;

        public SapphireContextBuilder(IServiceCollection serviceCollection)
        {
            this.serviceCollection = serviceCollection;
        }
        
        public SapphireContextBuilder AddModelConfiguration<T>() where T : class, ISapphireModelConfiguration
        {
            serviceCollection.AddTransient<ISapphireModelConfiguration, T>();
            return this;
        }
    }
}