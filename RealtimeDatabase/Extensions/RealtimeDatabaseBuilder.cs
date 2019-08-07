using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RealtimeDatabase.Internal;

namespace RealtimeDatabase.Extensions
{
    public class RealtimeDatabaseBuilder
    {
        private readonly IServiceCollection serviceCollection;

        public RealtimeDatabaseBuilder(IServiceCollection serviceCollection)
        {
            this.serviceCollection = serviceCollection;
        }

        public RealtimeDatabaseBuilder AddContext<TContextType>(
            Action<DbContextOptionsBuilder> dbContextOptions = null,
            string contextName = "Default")
            where TContextType : RealtimeDbContext
        {
            DbContextTypeContainer contextTypes = (DbContextTypeContainer)serviceCollection
                .FirstOrDefault(s => s.ServiceType == typeof(DbContextTypeContainer))?.ImplementationInstance;

            // ReSharper disable once PossibleNullReferenceException
            contextTypes.AddContext(contextName, typeof(TContextType));

            serviceCollection.AddDbContext<TContextType>(dbContextOptions, ServiceLifetime.Transient);

            return this;
        }
    }
}
