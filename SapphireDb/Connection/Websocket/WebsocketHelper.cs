using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using SapphireDb.Helper;

namespace SapphireDb.Connection.Websocket
{
    static class WebsocketHelper
    {
        public static CancellationToken token = CancellationToken.None;

        public static async Task<string> Receive(this WebSocket socket)
        {
            int bufferSize = 1024;
            byte[] buffer = new byte[bufferSize];
            int offset = 0;
            int free = buffer.Length;
            WebSocketReceiveResult result;

            while (true)
            {
                result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer, offset, free), token);
                offset += result.Count;
                free -= result.Count;

                if (result.EndOfMessage) break;

                if (free == 0)
                {
                    int newSize = buffer.Length + bufferSize;
                    byte[] newBuffer = new byte[newSize];
                    Array.Copy(buffer, 0, newBuffer, 0, offset);
                    buffer = newBuffer;
                    free = buffer.Length - offset;
                }
            }

            if (!result.CloseStatus.HasValue && result.MessageType == WebSocketMessageType.Text)
            {
                return Encoding.UTF8.GetString(buffer).Replace("\0", "");
            }

            return "";
        }

        public static async Task Send(this WebSocket socket, object message)
        {
            await socket.Send(JsonHelper.Serialize(message));
        }
        
        private static async Task Send(this WebSocket socket, string message)
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

        public static string GetCustomHeader(HttpRequest request, string header)
        {
            if (request.Headers.TryGetValue("sec-websocket-protocol", out StringValues customHeader) && customHeader.Count == 1)
            {
                string[] customHeaderParts = customHeader[0].Split(", ");

                int keyIndex = Array.FindIndex(customHeaderParts, value => value.Equals(header, StringComparison.InvariantCultureIgnoreCase));
                if (keyIndex != -1)
                {
                    return customHeaderParts[keyIndex + 1];
                }
            }

            return null;
        }
    }
}
