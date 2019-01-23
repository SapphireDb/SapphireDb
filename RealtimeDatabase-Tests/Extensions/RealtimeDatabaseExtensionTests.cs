using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using RealtimeDatabase;
using RealtimeDatabase.Extensions;
using RealtimeDatabase.Internal;
using RealtimeDatabase.Models;
using RealtimeDatabase.Models.Auth;
using Xunit;

namespace RealtimeDatabase_Tests.Extensions
{
    public class RealtimeDatabaseExtensionTests
    {
        private readonly ServiceCollection serviceCollection;

        public RealtimeDatabaseExtensionTests()
        {
            serviceCollection = new ServiceCollection();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void AddRealtimeDatabaseShouldAddServices(bool defaultConfiguration)
        {
            if (defaultConfiguration)
            {
                serviceCollection.AddRealtimeDatabase<RealtimeDbContext>();
            }
            else
            {
                serviceCollection.AddRealtimeDatabase<RealtimeDbContext>(options: new RealtimeDatabaseOptions());
            }

            Assert.Contains(serviceCollection, s => s.ServiceType == typeof(RealtimeDbContext) && s.Lifetime == ServiceLifetime.Transient);
            Assert.Contains(serviceCollection, s => s.ServiceType == typeof(RealtimeDatabaseOptions) && s.Lifetime == ServiceLifetime.Singleton);
            Assert.DoesNotContain(serviceCollection, s => s.ServiceType == typeof(AuthDbContextTypeContainer));
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void AddRealtimeAuthShouldAddAuthServices(bool authCommands, bool defaultIdentityOptionsAction)
        {
            if (authCommands)
            {
                serviceCollection.AddRealtimeDatabase<RealtimeDbContext>(options: new RealtimeDatabaseOptions()
                {
                    EnableAuthCommands = true
                });
            }
            else
            {
                serviceCollection.AddRealtimeDatabase<RealtimeDbContext>();
            }

            if (defaultIdentityOptionsAction)
            {
                serviceCollection.AddRealtimeAuth<RealtimeAuthContext<IdentityUser>, IdentityUser>(new JwtOptions("pw1234", "test"));
            }
            else
            {
                serviceCollection.AddRealtimeAuth<RealtimeAuthContext<IdentityUser>, IdentityUser>(new JwtOptions("pw1234", "test"),
                    identityOptionsAction: (opt) => {
                        opt.Password.RequireDigit = true;
                    });
            }

            Assert.Contains(serviceCollection, s => s.ServiceType == typeof(AuthDbContextTypeContainer) && s.Lifetime == ServiceLifetime.Singleton);
            Assert.Contains(serviceCollection, s => s.ServiceType == typeof(RealtimeAuthContext<IdentityUser>) && s.Lifetime == ServiceLifetime.Transient);
            Assert.Contains(serviceCollection, s => s.ServiceType == typeof(IRoleStore<IdentityRole>));
            Assert.Contains(serviceCollection, s => s.ServiceType == typeof(IUserStore<IdentityUser>) && s.Lifetime == ServiceLifetime.Transient);

            Assert.Contains(serviceCollection, s => s.ServiceType.Name == "LoginCommandHandler" && s.Lifetime == ServiceLifetime.Transient);
            Assert.Contains(serviceCollection, s => s.ServiceType.Name == "RenewCommandHandler" && s.Lifetime == ServiceLifetime.Transient);

            if (authCommands)
            {
                Assert.Contains(serviceCollection, s => s.ServiceType.Name == "SubscribeUsersCommandHandler" && s.Lifetime == ServiceLifetime.Transient);
            }
            else
            {
                Assert.DoesNotContain(serviceCollection, s => s.ServiceType.Name == "SubscribeUsersCommandHandler");
            }
        }
    }
}
