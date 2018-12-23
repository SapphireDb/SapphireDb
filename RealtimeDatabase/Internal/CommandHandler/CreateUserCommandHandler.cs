using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class CreateUserCommandHandler : AuthCommandHandlerBase, ICommandHandler<CreateUserCommand>
    {
        private readonly AuthDbContextTypeContainer contextTypeContainer;
        private readonly RoleManager<IdentityRole> roleManager;

        public CreateUserCommandHandler(AuthDbContextAccesor authDbContextAccesor, 
            AuthDbContextTypeContainer contextTypeContainer, IServiceProvider serviceProvider, RoleManager<IdentityRole> roleManager)
            : base(authDbContextAccesor, serviceProvider)
        {
            this.contextTypeContainer = contextTypeContainer;
            this.roleManager = roleManager;
        }

        public async Task Handle(WebsocketConnection websocketConnection, CreateUserCommand command)
        {
            IRealtimeAuthContext context = GetContext();
            dynamic usermanager = serviceProvider.GetService(contextTypeContainer.UserManagerType);

            try
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

                IdentityResult result = await usermanager.CreateAsync(newUser, command.Password);

                if (result.Succeeded)
                {
                    foreach (string roleStr in command.Roles)
                    {
                        if (!await roleManager.RoleExistsAsync(roleStr))
                        {
                            await roleManager.CreateAsync(new IdentityRole(roleStr));
                        }
                    }

                    await usermanager.AddToRolesAsync(newUser, command.Roles);

                    Dictionary<string, object> newUserData = ModelHelper.GenerateUserData(newUser);
                    newUserData["Roles"] =
                        await (dynamic)contextTypeContainer.UserManagerType.GetMethod("GetRolesAsync").Invoke(usermanager, new object[] { newUser });

                    await SendMessage(websocketConnection, new CreateUserResponse()
                    {
                        ReferenceId = command.ReferenceId,
                        NewUser = newUserData
                    });
                }
                else
                {
                    await SendMessage(websocketConnection, new CreateUserResponse()
                    {
                        ReferenceId = command.ReferenceId,
                        IdentityErrors = result.Errors
                    });
                }
            }
            catch (Exception ex)
            {
                await SendMessage(websocketConnection, new CreateUserResponse()
                {
                    ReferenceId = command.ReferenceId,
                    Error = ex
                });
            }
        }
    }
}
