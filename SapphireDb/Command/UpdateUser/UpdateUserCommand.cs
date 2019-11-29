using SapphireDb.Command.CreateUser;

namespace SapphireDb.Command.UpdateUser
{
    public class UpdateUserCommand : CreateUserCommand
    {
        public string Id { get; set; }
    }
}
