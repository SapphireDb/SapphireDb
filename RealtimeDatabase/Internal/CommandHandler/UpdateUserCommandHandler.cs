using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Linq;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Connection;
using RealtimeDatabase.Helper;
using RealtimeDatabase.Models;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class UpdateUserCommandHandler : AuthCommandHandlerBase, ICommandHandler<UpdateUserCommand>
    {
        private readonly RealtimeConnectionManager connectionManager;
        private readonly AuthDbContextTypeContainer contextTypeContainer;
        private readonly RoleManager<IdentityRole> roleManager;

        public UpdateUserCommandHandler(AuthDbContextAccesor authDbContextAccessor, RealtimeConnectionManager connectionManager,
            AuthDbContextTypeContainer contextTypeContainer, IServiceProvider serviceProvider, RoleManager<IdentityRole> roleManager)
            : base(authDbContextAccessor, serviceProvider)
        {
            this.connectionManager = connectionManager;
            this.contextTypeContainer = contextTypeContainer;
            this.roleManager = roleManager;
        }

        public async Task<ResponseBase> Handle(HttpInformation context, UpdateUserCommand command)
        {
            try
            {
                dynamic usermanager = serviceProvider.GetService(contextTypeContainer.UserManagerType);
                return await UpdateUser(usermanager, command);
            }
            catch (Exception ex)
            {
                return command.CreateExceptionResponse<UpdateUserResponse>(ex);
            }
        }

        private async Task<ResponseBase> UpdateUser(dynamic usermanager, UpdateUserCommand command)
        {
            IRealtimeAuthContext context = GetContext();

            dynamic user = await usermanager.FindByIdAsync(command.Id);

            if (user != null)
            {
                return await HandleUserUpdate(user, command, usermanager, context);
            }
            else
            {
                return command.CreateExceptionResponse<UpdateUserResponse>("No user with this id was found.");
            }
        }

        private async Task<ResponseBase> HandleUserUpdate(dynamic user, UpdateUserCommand command, dynamic usermanager, IRealtimeAuthContext context)
        {
            SetUserProperties(user, command, usermanager);

            IdentityResult result = await usermanager.UpdateAsync(user);

            if (result.Succeeded)
            {
                HandleRolesUpdate(command, usermanager, user);

                MessageHelper.SendUsersUpdate(context, contextTypeContainer, usermanager, connectionManager);
                MessageHelper.SendRolesUpdate(context, connectionManager);

                return new UpdateUserResponse()
                {
                    ReferenceId = command.ReferenceId,
                    NewUser = await ModelHelper.GenerateUserData(user, contextTypeContainer, usermanager)
                };
            }
            else
            {
                return new UpdateUserResponse()
                {
                    ReferenceId = command.ReferenceId,
                    IdentityErrors = result.Errors
                };
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
