using System.Net.Sockets;

namespace ImplementingDbDriver;

public static class ConnectSocket
{
    public static void Run()
    {
        var socket = new Socket(SocketType.Stream, ProtocolType.Tcp)
        {
            NoDelay = true
        };

        socket.Connect("127.0.0.1", 1000);

        socket.Send(new byte[] { 1, 2, 3 });

        var buf = new byte[3];
        int received = socket.Receive(buf);
    }
}
