namespace RealtimeDatabase.Models.Commands
{
    class LoginCommand : CommandBase
    {
        public string Username { get; set; }

        public string Password { get; set; }
    }
}
