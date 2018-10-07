using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RealtimeDatabase
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RealtimeAuthorizeAttribute : Attribute
    {
        public string[] RolesRead { get; set; }

        public string[] RolesWrite { get; set; }

        public string[] RolesDelete { get; set; }

        public RealtimeAuthorizeAttribute()
        {

        }

        public RealtimeAuthorizeAttribute(string roles)
        {
            RolesRead = RolesWrite = RolesDelete = roles.Split(',').Select(r => r.Trim()).ToArray();
        }

        public RealtimeAuthorizeAttribute(string rolesRead, string rolesWrite, string rolesDelete)
        {
            RolesRead = rolesRead.Split(',').Select(r => r.Trim()).ToArray();
            RolesWrite = rolesWrite.Split(',').Select(r => r.Trim()).ToArray();
            RolesDelete = rolesDelete.Split(',').Select(r => r.Trim()).ToArray();
        }

        public enum OperationType
        {
            Read, Write, Delete
        }
    }
}
