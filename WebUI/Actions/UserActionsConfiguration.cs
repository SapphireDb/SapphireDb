using SapphireDb.Models.SapphireApiBuilder;

namespace WebUI.Actions
{
    public class UserActionsConfiguration : ISapphireActionConfiguration<UserActions>
    {
        public void Configure(SapphireActionHandlerBuilder<UserActions> actionHandlerBuilder)
        {
            actionHandlerBuilder.Action("Login").AddActionAuth(function: info => true);
        }
    }
}