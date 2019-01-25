using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Linq;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class CreateUserCommandHandler : AuthCommandHandlerBase, ICommandHandler<CreateUserCommand>
    {
        private readonly WebsocketConnectionManager connectionManager;
        private readonly AuthDbContextTypeContainer contextTypeContainer;
        private readonly RoleManager<IdentityRole> roleManager;

        public CreateUserCommandHandler(AuthDbContextAccesor authDbContextAccessor, WebsocketConnectionManager connectionManager,
            AuthDbContextTypeContainer contextTypeContainer, IServiceProvider serviceProvider, RoleManager<IdentityRole> roleManager)
            : base(authDbContextAccessor, serviceProvider)
        {
            this.connectionManager = connectionManager;
            this.contextTypeContainer = contextTypeContainer;
            this.roleManager = roleManager;
        }

        public async Task Handle(WebsocketConnection websocketConnection, CreateUserCommand command)
        {
            dynamic usermanager = serviceProvider.GetService(contextTypeContainer.UserManagerType);

            try
            {
                await SaveUserToDb(command, usermanager, websocketConnection);
            }
            catch (Exception ex)
            {
                await websocketConnection.Send(new CreateUserResponse()
                {
                    ReferenceId = command.ReferenceId,
                    Error = ex
                });
            }
        }

        private async Task SaveUserToDb(CreateUserCommand command, dynamic usermanager, WebsocketConnection websocketConnection)
        {
            dynamic newUser = CreateUserObject(command);
            IdentityResult result = await usermanager.CreateAsync(newUser, command.Password);

            if (result.Succeeded)
            {
                await SetUserRoles(command, usermanager, newUser);

                await websocketConnection.Send(new CreateUserResponse()
                {
                    ReferenceId = command.ReferenceId,
                    NewUser = await ModelHelper.GenerateUserData(newUser, contextTypeContainer, usermanager)
                });

                IRealtimeAuthContext context = GetContext();

                await MessageHelper.SendUsersUpdate(context, contextTypeContainer, usermanager, connectionManager);
                await MessageHelper.SendRolesUpdate(context, connectionManager);
            }
            else
            {
                await websocketConnection.Send(new CreateUserResponse()
                {
                    ReferenceId = command.ReferenceId,
                    IdentityErrors = result.Errors
                });
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
