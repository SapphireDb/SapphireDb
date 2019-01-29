namespace RealtimeDatabase.Models.Commands
{
    public class UpdateRoleCommand : CommandBase
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }
}
