namespace RealtimeDatabase.Models.Commands
{
    class RenewCommand : CommandBase
    {
        public string UserId { get; set; }

        public string RefreshToken { get; set; }
    }
}
