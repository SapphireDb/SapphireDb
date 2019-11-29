using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SapphireDb.Internal;

namespace SapphireDb.Extensions
{
    public class SapphireDatabaseBuilder
    {
        private readonly IServiceCollection serviceCollection;

        public SapphireDatabaseBuilder(IServiceCollection serviceCollection)
        {
            this.serviceCollection = serviceCollection;
        }

        public SapphireDatabaseBuilder AddContext<TContextType>(
            Action<DbContextOptionsBuilder> dbContextOptions = null,
            string contextName = "Default")
            where TContextType : SapphireDbContext
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
