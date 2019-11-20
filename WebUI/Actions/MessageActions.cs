using RealtimeDatabase.Models.Actions;
using System.Threading.Tasks;
using RealtimeDatabase.Connection;

namespace WebUI.Actions
{
    public class MessageActions : ActionHandlerBase
    {
        private readonly RealtimeMessageSender MessageSender;

        public MessageActions(RealtimeMessageSender messageSender)
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
