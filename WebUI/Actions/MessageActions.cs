using System.Threading.Tasks;
using SapphireDb.Actions;
using SapphireDb.Connection;

namespace WebUI.Actions
{
    public class MessageActions : ActionHandlerBase
    {
        private readonly SapphireMessageSender MessageSender;

        public MessageActions(SapphireMessageSender messageSender)
        {
            MessageSender = messageSender;
        }

        public void Publish(string message)
        {
            MessageSender.Send(message);
        }

        public void SendToAdmin(string message)
        {
            MessageSender.Send(c => c.Information.User.IsInRole("admin"), message);
        }
    }
}
