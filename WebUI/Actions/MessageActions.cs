using RealtimeDatabase.Models.Actions;
using RealtimeDatabase.Websocket;
using System.Threading.Tasks;

namespace WebUI.Actions
{
    public class MessageActions : ActionHandlerBase
    {
        private readonly RealtimeMessageSender MessageSender;

        public MessageActions(RealtimeMessageSender messageSender)
        {
            MessageSender = messageSender;
        }

        public async Task Publish(string message)
        {
            await MessageSender.Send(message);
        }

        public async Task SendToAdmin(string message)
        {
            await MessageSender.Send(c => c.HttpContext.User.IsInRole("admin"), message);
        }
    }
}
