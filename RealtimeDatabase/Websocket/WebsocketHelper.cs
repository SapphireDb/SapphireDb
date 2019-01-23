using RealtimeDatabase.Internal;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RealtimeDatabase.Websocket
{
    static class WebsocketHelper
    {
        public static CancellationToken token = CancellationToken.None;

        public static async Task<string> Receive(this WebSocket socket)
        {
            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[4096]);

            WebSocketReceiveResult received = await socket.ReceiveAsync(buffer, token);

            if (!received.CloseStatus.HasValue && received.MessageType == WebSocketMessageType.Text)
            {
                return Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count).Replace("\0", "");
            }

            return "";
        }

        public static async Task Send(this WebSocket socket, string message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                ArraySegment<byte> buffer = new ArraySegment<byte>(data);

                if (socket != null && socket.State == WebSocketState.Open)
                {
                    await socket.SendAsync(buffer, WebSocketMessageType.Text, true, token);
                }
            }
            catch
            {
                // ignored
            }
        }

        public static async Task Send(this WebSocket socket, object message)
        {
            await socket.Send(JsonHelper.Serialize(message));
        }
    }
}
