using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SapphireDb.Connection;
using SapphireDb.Helper;
using SapphireDb.Internal;

namespace SapphireDb.Extensions
{
    public class SapphireDatabaseBuilder
    {
        public readonly IServiceCollection serviceCollection;

        public SapphireDatabaseBuilder(IServiceCollection serviceCollection)
        {
            this.serviceCollection = serviceCollection;
        }

        public SapphireDatabaseBuilder AddContext<TContextType>(
            Action<IServiceProvider, DbContextOptionsBuilder> optionsActions = null,
            string contextName = "Default")
            where TContextType : DbContext
        {
            DbContextTypeContainer contextTypes = (DbContextTypeContainer) serviceCollection
                .FirstOrDefault(s => s.ServiceType == typeof(DbContextTypeContainer))?.ImplementationInstance;
            
            contextTypes?.AddContext(contextName, typeof(TContextType));

            serviceCollection.AddDbContext<TContextType>((serviceProvider, dbContextOptions) =>
            {
                SapphireDbInterceptor dbInterceptor = serviceProvider.GetRequiredService<SapphireDbInterceptor>();
                dbContextOptions.AddInterceptors(dbInterceptor);
                
                optionsActions?.Invoke(serviceProvider, dbContextOptions);
            }, ServiceLifetime.Transient);

            return this;
        }
        
        public SapphireDatabaseBuilder AddContext<TContextType>(
            Action<DbContextOptionsBuilder> optionsActions = null,
            string contextName = "Default")
            where TContextType : DbContext
        {
            AddContext<TContextType>((_, optionsBuilder) => optionsActions?.Invoke(optionsBuilder), contextName);
            return this;
        }

        public SapphireDatabaseBuilder AddTopicConfiguration(string topic,
            Func<IConnectionInformation, bool> canSubscribe,
            Func<IConnectionInformation, bool> canPublish)
        {
            MessageTopicHelper.RegisteredTopicAuthFunctions.Add(topic, new Tuple<Func<IConnectionInformation, bool>,
                Func<IConnectionInformation, bool>>(canSubscribe, canPublish));
            return this;
        }

        public SapphireDatabaseBuilder AddMessageFilter(string name,
            Func<IConnectionInformation, object[], bool> filter)
        {
            SapphireMessageSender.RegisteredMessageFilter.Add(name.ToLowerInvariant(), filter);
            return this;
        }

        public SapphireDatabaseBuilder AddMessageFilter(string name, Func<IConnectionInformation, bool> filter)
        {
            SapphireMessageSender.RegisteredMessageFilter.Add(name.ToLowerInvariant(), filter);
            return this;
        }
    }
}