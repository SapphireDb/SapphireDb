using RealtimeDatabase.Command.CreateUser;

namespace RealtimeDatabase.Command.UpdateUser
{
    public class UpdateUserCommand : CreateUserCommand
    {
        public string Id { get; set; }
    }
}
