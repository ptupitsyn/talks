using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Grpc.Core;

namespace ImplementingDbDriver;

#pragma warning disable CS4014 // Task is not awaited.

/// <summary>
/// Compares "Hello World" exchange using HTTP, gRPC and raw Socket APIs.
/// </summary>
[MemoryDiagnoser]
public class ProtocolBenchmarks
{
    private const int GrpcPort = 30051;
    private const int SocketPort = 10900;
    private const string UserName = "John Doe";

    public static void Run()
    {
        // var r = new ProtocolBenchmarks();
        // r.GlobalSetup();
        // Console.WriteLine(r.Grpc().GetAwaiter().GetResult());
        // Console.WriteLine(r.Socket().GetAwaiter().GetResult());

        BenchmarkRunner.Run<ProtocolBenchmarks>();
    }

    [GlobalSetup]
    public void GlobalSetup()
    {
        // gRPC.
        var grpcServer = new Server
        {
            Services = { Greeter.BindService(new GrpcServer()) },
            Ports = { new ServerPort("localhost", GrpcPort, ServerCredentials.Insecure) }
        };
        grpcServer.Start();

        // Socket.
        var listenerSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        listenerSocket.Bind(new IPEndPoint(IPAddress.Loopback, SocketPort));
        listenerSocket.Listen(backlog: 1);

        Task.Run(async () =>
        {
            while (true)
            {
                using var clientSocket = await listenerSocket.AcceptAsync();
                var userName = await ReceiveString(clientSocket);
                var greeting = "Hello " + userName;
                await SendString(clientSocket, greeting);
            }
        });
    }

    [Benchmark]
    public async Task<string> Socket()
    {
        using var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        await socket.ConnectAsync("localhost", SocketPort);

        await SendString(socket, UserName);
        return await ReceiveString(socket);
    }

    [Benchmark]
    public void Http()
    {
        // TODO

    }

    [Benchmark]
    public async Task<string> Grpc()
    {
        var channel = new Channel("127.0.0.1:30051", ChannelCredentials.Insecure);

        var client = new Greeter.GreeterClient(channel);
        var reply = await client.SayHelloAsync(new HelloRequest { Name = UserName });

        await channel.ShutdownAsync();

        return reply.Message;
    }

    private static async Task SendString(Socket socket, string val)
    {
        await socket.SendAsync(BitConverter.GetBytes(val.Length), SocketFlags.None);
        await socket.SendAsync(Encoding.UTF8.GetBytes(val), SocketFlags.None);
    }

    private static async Task<string> ReceiveString(Socket socket)
    {
        var pooledArr = ArrayPool<byte>.Shared.Rent(100);

        await socket.ReceiveAsync(pooledArr.AsMemory()[..4], SocketFlags.None);

        var len = BitConverter.ToInt32(pooledArr);

        var buf = pooledArr.AsMemory()[..len];
        await socket.ReceiveAsync(buf, SocketFlags.None);

        var res = Encoding.UTF8.GetString(buf.Span);
        ArrayPool<byte>.Shared.Return(pooledArr);

        return res;
    }

    private class GrpcServer : Greeter.GreeterBase
    {
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply { Message = "Hello " + request.Name });
        }
    }
}
