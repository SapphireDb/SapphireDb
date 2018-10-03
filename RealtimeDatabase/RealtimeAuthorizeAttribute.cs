using System;
using System.Collections.Generic;
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
            RolesRead = RolesWrite = RolesDelete = roles.Split(',');
        }

        public RealtimeAuthorizeAttribute(string rolesRead, string rolesWrite, string rolesDelete)
        {
            RolesRead = rolesRead.Split(',');
            RolesWrite = rolesWrite.Split(',');
            RolesDelete = rolesDelete.Split(',');
        }

        public enum OperationType
        {
            Read, Write, Delete
        }
    }
}
