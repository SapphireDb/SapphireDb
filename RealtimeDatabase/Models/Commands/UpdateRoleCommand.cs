namespace RealtimeDatabase.Models.Commands
{
    class UpdateRoleCommand : CommandBase
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }
}
