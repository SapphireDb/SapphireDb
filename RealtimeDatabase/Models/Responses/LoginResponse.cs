using Microsoft.AspNetCore.Identity;
using RealtimeDatabase.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RealtimeDatabase.Models.Responses
{
    class LoginResponse : ResponseBase
    {
        public LoginResponse()
        {
            UserData = new Dictionary<string, object>();
        }

        public string AuthToken { get; set; }

        public string RefreshToken { get; set; }

        public double ValidFor { get; set; }

        public DateTime ExpiresAt { get; set; }

        public Dictionary<string, object> UserData { get; set; }

        public void GenerateUserData(IdentityUser identityUser)
        {
            Type t = identityUser.GetType();

            IEnumerable<PropertyInfo> properties =
                t.GetProperties().Where(p => p.GetCustomAttribute<AuthUserInformationAttribute>() != null
                || p.Name == "Id" || p.Name == "UserName" || p.Name == "Email");

            foreach (PropertyInfo property in properties)
            {
                if (property.Name != "Roles")
                {
                    UserData[property.Name] = property.GetValue(identityUser);
                } else
                {
                    UserData["_Roles"] = property.GetValue(identityUser);
                }
            }
        }
    }
}
