using System.Buffers;
using System.Net.Sockets;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Grpc.Core;

namespace ImplementingDbDriver;

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
        using var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
    }

    [Benchmark]
    public async Task<string> Socket()
    {
        using var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        await socket.ConnectAsync("localhost", SocketPort);

        await socket.SendAsync(BitConverter.GetBytes(UserName.Length), SocketFlags.None);
        await socket.SendAsync(Encoding.UTF8.GetBytes(UserName), SocketFlags.None);

        var lenBytes = new byte[4];
        await socket.ReceiveAsync(lenBytes, SocketFlags.None);

        var len = BitConverter.ToInt32(lenBytes);

        var pooledArr = ArrayPool<byte>.Shared.Rent(len);
        try
        {
            var buf = pooledArr.AsMemory()[len..];
            await socket.ReceiveAsync(buf, SocketFlags.None);
            var res = Encoding.UTF8.GetString(buf.Span);
            return res;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(pooledArr);
        }
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

    private class GrpcServer : Greeter.GreeterBase
    {
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply { Message = "Hello " + request.Name });
        }
    }
}
