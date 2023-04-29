using System.Net;
using System.Net.Sockets;

namespace PushCar.Utils
{
    public static class SocketExtensions
    {
        public static void BindAndListen(this Socket socket, int port, int backlog = 1000)
        {
            socket.Bind(new IPEndPoint(IPAddress.Any, port));
            socket.Listen(backlog);
        }
    }
}