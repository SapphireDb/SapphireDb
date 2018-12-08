using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Internal
{
    public class AuthDbContextTypeContainer : DbContextTypeContainer
    {
        public Type UserManagerType { get; set; }
    }
}
