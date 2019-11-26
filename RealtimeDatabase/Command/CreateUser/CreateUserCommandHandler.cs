using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Linq;
using RealtimeDatabase.Connection;
using RealtimeDatabase.Helper;
using RealtimeDatabase.Internal;
using RealtimeDatabase.Models;

namespace RealtimeDatabase.Command.CreateUser
{
    class CreateUserCommandHandler : AuthCommandHandlerBase, ICommandHandler<CreateUserCommand>
    {
        private readonly RealtimeConnectionManager connectionManager;
        private readonly AuthDbContextTypeContainer contextTypeContainer;
        private readonly RoleManager<IdentityRole> roleManager;

        public CreateUserCommandHandler(AuthDbContextAccesor authDbContextAccessor, RealtimeConnectionManager connectionManager,
            AuthDbContextTypeContainer contextTypeContainer, IServiceProvider serviceProvider, RoleManager<IdentityRole> roleManager)
            : base(authDbContextAccessor, serviceProvider)
        {
            this.connectionManager = connectionManager;
            this.contextTypeContainer = contextTypeContainer;
            this.roleManager = roleManager;
        }

        public async Task<ResponseBase> Handle(HttpInformation context, CreateUserCommand command)
        {
            dynamic usermanager = serviceProvider.GetService(contextTypeContainer.UserManagerType);

            try
            {
                return await SaveUserToDb(command, usermanager);
            }
            catch (Exception ex)
            {
                return new CreateUserResponse()
                {
                    ReferenceId = command.ReferenceId,
                    Error = ex
                };
            }
        }

        private async Task<ResponseBase> SaveUserToDb(CreateUserCommand command, dynamic usermanager)
        {
            dynamic newUser = CreateUserObject(command);
            IdentityResult result = await usermanager.CreateAsync(newUser, command.Password);

            if (result.Succeeded)
            {
                await SetUserRoles(command, usermanager, newUser);

                IRealtimeAuthContext context = GetContext();

                MessageHelper.SendUsersUpdate(context, contextTypeContainer, usermanager, connectionManager);
                MessageHelper.SendRolesUpdate(context, connectionManager);

                return new CreateUserResponse()
                {
                    ReferenceId = command.ReferenceId,
                    NewUser = await ModelHelper.GenerateUserData(newUser, contextTypeContainer, usermanager)
                };
            }
            else
            {
                return new CreateUserResponse()
                {
                    ReferenceId = command.ReferenceId,
                    IdentityErrors = result.Errors
                };
            }
        }

        private async Task SetUserRoles(CreateUserCommand command, dynamic usermanager, dynamic newUser)
        {
            foreach (string roleStr in command.Roles)
            {
                if (!await roleManager.RoleExistsAsync(roleStr))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleStr));
                }
            }

            await usermanager.AddToRolesAsync(newUser, command.Roles);
        }

        private dynamic CreateUserObject(CreateUserCommand command)
        {
            dynamic newUser = Activator.CreateInstance(contextTypeContainer.UserType);
            newUser.Email = command.Email;
            newUser.UserName = command.UserName;

            foreach (KeyValuePair<string, JValue> keyValue in command.AdditionalData)
            {
                PropertyInfo pi = contextTypeContainer.UserType.GetProperty(keyValue.Key,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (pi != null && pi.DeclaringType == contextTypeContainer.UserType)
                {
                    pi.SetValue(newUser, keyValue.Value.ToObject(pi.PropertyType));
                }
            }

            return newUser;
        }
    }
}
