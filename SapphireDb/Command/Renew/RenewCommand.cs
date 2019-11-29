namespace SapphireDb.Command.Renew
{
    public class RenewCommand : CommandBase
    {
        public string UserId { get; set; }

        public string RefreshToken { get; set; }
    }
}
