using System;

namespace RealtimeDatabase.Internal
{
    public class AuthDbContextTypeContainer : DbContextTypeContainer
    {
        public Type UserType { get; set; }

        public Type UserManagerType { get; set; }
    }
}
