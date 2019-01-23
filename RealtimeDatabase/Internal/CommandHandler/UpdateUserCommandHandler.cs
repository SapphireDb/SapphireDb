using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Linq;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class UpdateUserCommandHandler : AuthCommandHandlerBase, ICommandHandler<UpdateUserCommand>
    {
        private readonly WebsocketConnectionManager connectionManager;
        private readonly AuthDbContextTypeContainer contextTypeContainer;
        private readonly RoleManager<IdentityRole> roleManager;

        public UpdateUserCommandHandler(AuthDbContextAccesor authDbContextAccesor, WebsocketConnectionManager connectionManager,
            AuthDbContextTypeContainer contextTypeContainer, IServiceProvider serviceProvider, RoleManager<IdentityRole> roleManager)
            : base(authDbContextAccesor, serviceProvider)
        {
            this.connectionManager = connectionManager;
            this.contextTypeContainer = contextTypeContainer;
            this.roleManager = roleManager;
        }

        public async Task Handle(WebsocketConnection websocketConnection, UpdateUserCommand command)
        {
            try
            {
                dynamic usermanager = serviceProvider.GetService(contextTypeContainer.UserManagerType);
                await UpdateUser(usermanager, command, websocketConnection);
            }
            catch (Exception ex)
            {
                await websocketConnection.SendException<UpdateUserResponse>(command, ex);
            }
        }

        private async Task UpdateUser(dynamic usermanager, UpdateUserCommand command, WebsocketConnection websocketConnection)
        {
            IRealtimeAuthContext context = GetContext();

            dynamic user = await usermanager.FindByIdAsync(command.Id);

            if (user != null)
            {
                await HandleUserUpdate(user, command, usermanager, websocketConnection, context);
            }
            else
            {
                await websocketConnection.SendException<UpdateUserResponse>(command, "No user with this id was found.");
            }
        }

        private async Task HandleUserUpdate(dynamic user, UpdateUserCommand command, dynamic usermanager, WebsocketConnection websocketConnection,
            IRealtimeAuthContext context)
        {
            SetUserProperties(user, command, usermanager);

            IdentityResult result = await usermanager.UpdateAsync(user);

            if (result.Succeeded)
            {
                HandleRolesUpdate(command, usermanager, user);

                await websocketConnection.Send(new UpdateUserResponse()
                {
                    ReferenceId = command.ReferenceId,
                    NewUser = await ModelHelper.GenerateUserData(user, contextTypeContainer, usermanager)
                });

                await MessageHelper.SendUsersUpdate(context, contextTypeContainer, usermanager, connectionManager);
                await MessageHelper.SendRolesUpdate(context, connectionManager);
            }
            else
            {
                await websocketConnection.Send(new UpdateUserResponse()
                {
                    ReferenceId = command.ReferenceId,
                    IdentityErrors = result.Errors
                });
            }
        }

        private async Task HandleRolesUpdate(UpdateUserCommand command, dynamic usermanager, dynamic user)
        {
            if (command.Roles != null)
            {
                List<string> originalRoles =
                    await contextTypeContainer.UserManagerType.GetMethod("GetRolesAsync").Invoke(usermanager, new object[] { user });
                List<string> newRoles = command.Roles.Where(r => originalRoles.All(or => or != r)).ToList();
                IEnumerable<string> deletedRoles = originalRoles.Where(or => command.Roles.All(r => r != or));

                foreach (string roleStr in newRoles)
                {
                    if (!await roleManager.RoleExistsAsync(roleStr))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleStr));
                    }
                }

                await usermanager.RemoveFromRolesAsync(user, deletedRoles);
                await usermanager.AddToRolesAsync(user, newRoles);
            }
        }

        private void SetUserProperties(dynamic user, UpdateUserCommand command, dynamic usermanager)
        {
            if (!string.IsNullOrEmpty(command.Email))
            {
                user.Email = command.Email;
            }

            if (!string.IsNullOrEmpty(command.UserName))
            {
                user.UserName = command.UserName;
            }

            if (command.AdditionalData != null)
            {
                foreach (KeyValuePair<string, JValue> keyValue in command.AdditionalData)
                {
                    PropertyInfo pi = contextTypeContainer.UserType.GetProperty(keyValue.Key,
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                    if (pi != null && pi.DeclaringType == contextTypeContainer.UserType)
                    {
                        pi.SetValue(user, keyValue.Value.ToObject(pi.PropertyType));
                    }
                }
            }

            if (!string.IsNullOrEmpty(command.Password))
            {
                user.PasswordHash = usermanager.PasswordHasher.HashPassword(user, command.Password);
            }
        }
    }
}
