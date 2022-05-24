using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Grpc.Core;

namespace ImplementingDbDriver;

/// <summary>
/// Compares "Hello World" exchange on established connection using gRPC and Socket APIs.
///
/// | Method |     Mean | Ratio | Allocated |
/// |------- |---------:|------:|----------:|
/// | Socket | 19.43 us |  1.00 |     644 B |
/// |   Grpc | 86.27 us |  4.44 |   3,632 B |
/// </summary>
[MemoryDiagnoser]
public class ProtocolBenchmarksEstablishedConnection
{
    private const int GrpcPort = 10001;
    private const int SocketPort = 10003;
    private const string UserName = "John Doe";

    private Greeter.GreeterClient _grpcClient = null!;
    private Socket _socketClient = null!;

    public static void Run()
    {
        // var r = new ProtocolBenchmarksEstablishedConnection();
        // r.GlobalSetup().GetAwaiter().GetResult();
        // Console.WriteLine(r.Grpc().GetAwaiter().GetResult());
        // Console.WriteLine(r.Socket().GetAwaiter().GetResult());
        //
        BenchmarkRunner.Run<ProtocolBenchmarksEstablishedConnection>();
    }

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        // gRPC.
        var grpcServer = new Server
        {
            Services = { Greeter.BindService(new GrpcServer()) },
            Ports = { new ServerPort("localhost", GrpcPort, ServerCredentials.Insecure) }
        };
        grpcServer.Start();

        var channel = new Channel("127.0.0.1:" + GrpcPort, ChannelCredentials.Insecure);
        _grpcClient = new Greeter.GreeterClient(channel);


        // Socket.
        var listenerSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        listenerSocket.Bind(new IPEndPoint(IPAddress.Loopback, SocketPort));
        listenerSocket.Listen(backlog: 1);

        Task.Run(async () =>
        {
            while (true)
            {
                using var clientSocket = await listenerSocket.AcceptAsync();
                while (true)
                {
                    var userName = await ReceiveString(clientSocket);
                    var greeting = "Hello " + userName;
                    await SendString(clientSocket, greeting);
                }
            }
        });

        _socketClient = new Socket(SocketType.Stream, ProtocolType.Tcp);
        await _socketClient.ConnectAsync("localhost", SocketPort);
    }

    [Benchmark(Baseline = true)]
    public async Task<string> Socket()
    {
        await SendString(_socketClient, UserName);
        return await ReceiveString(_socketClient);
    }

    [Benchmark]
    public async Task<string> Grpc()
    {
        var reply = await _grpcClient.SayHelloAsync(new HelloRequest { Name = UserName });

        return reply.Message;
    }

    private static async Task SendString(Socket socket, string val)
    {
        var pooledArr = ArrayPool<byte>.Shared.Rent(100);

        try
        {
            BitConverter.TryWriteBytes(pooledArr.AsSpan(), val.Length);
            var byteLen = Encoding.UTF8.GetBytes(val, pooledArr.AsSpan()[4..]);

            await socket.SendAsync(pooledArr.AsMemory()[..(byteLen + 4)], SocketFlags.None);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(pooledArr);
        }
    }

    private static async Task<string> ReceiveString(Socket socket)
    {
        var pooledArr = ArrayPool<byte>.Shared.Rent(100);

        try
        {
            await socket.ReceiveAsync(pooledArr.AsMemory()[..4], SocketFlags.None);

            var len = BitConverter.ToInt32(pooledArr);

            var buf = pooledArr.AsMemory()[..len];
            await socket.ReceiveAsync(buf, SocketFlags.None);

            return Encoding.UTF8.GetString(buf.Span);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(pooledArr);
        }
    }

    private class GrpcServer : Greeter.GreeterBase
    {
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply { Message = "Hello " + request.Name });
        }
    }
}
