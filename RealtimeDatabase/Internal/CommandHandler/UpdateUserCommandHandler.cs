using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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

        private void SetUserProperties(dynamic user, UpdateUserCommand command, dynamic usermanager)
        {
            if (!String.IsNullOrEmpty(command.Email))
            {
                user.Email = command.Email;
            }

            if (!String.IsNullOrEmpty(command.UserName))
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

            if (!String.IsNullOrEmpty(command.Password))
            {
                user.PasswordHash = usermanager.PasswordHasher.HashPassword(user, command.Password);
            }
        }

        private async Task HandleRolesUpdate(UpdateUserCommand command, dynamic usermanager, dynamic user)
        {
            if (command.Roles != null)
            {
                List<string> originalRoles =
                    await(dynamic)contextTypeContainer.UserManagerType.GetMethod("GetRolesAsync").Invoke(usermanager, new object[] { user });
                IEnumerable<string> newRoles = command.Roles.Where(r => !originalRoles.Any(or => or == r));
                IEnumerable<string> deletedRoles = originalRoles.Where(or => !command.Roles.Any(r => r == or));

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

        public async Task Handle(WebsocketConnection websocketConnection, UpdateUserCommand command)
        {
            IRealtimeAuthContext context = GetContext();
            dynamic usermanager = serviceProvider.GetService(contextTypeContainer.UserManagerType);

            try
            {
                dynamic user = await usermanager.FindByIdAsync(command.Id);

                if (user != null)
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
                else
                {
                    await websocketConnection.Send(new UpdateUserResponse()
                    {
                        Error = new Exception("No user with this id was found.")
                    });
                }
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
    }
}
