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
/// Compares "Hello World" exchange using HTTP, gRPC and Socket APIs.
///
/// | Method |      Mean |    Error |   StdDev |  Gen 0 |  Gen 1 |  Gen 2 | Allocated |
/// |------- |----------:|---------:|---------:|-------:|-------:|-------:|----------:|
/// | Socket |  77.31 us | 0.558 us | 0.522 us | 0.7324 |      - |      - |      5 KB |
/// |   Http | 180.03 us | 3.556 us | 4.497 us | 8.7891 | 2.9297 | 0.4883 |     48 KB |
/// |   Grpc | 320.92 us | 1.226 us | 1.087 us | 0.4883 |      - |      - |      4 KB |
/// </summary>
[MemoryDiagnoser]
public class ProtocolBenchmarks
{
    private const int GrpcPort = 10001;
    private const int HttpPort = 10002;
    private const int SocketPort = 10003;
    private const string UserName = "John Doe";

    public static void Run()
    {
        // var r = new ProtocolBenchmarks();
        // r.GlobalSetup();
        // Console.WriteLine(r.Grpc().GetAwaiter().GetResult());
        // Console.WriteLine(r.Socket().GetAwaiter().GetResult());
        // Console.WriteLine(r.Http().GetAwaiter().GetResult());

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

        // Http.
        var httpListener = new HttpListener
        {
            Prefixes = { $"http://localhost:{HttpPort}/" }
        };

        httpListener.Start();

        Task.Run(async () =>
        {
            while (true)
            {
                var ctx = await httpListener.GetContextAsync();
                var reader = new StreamReader(ctx.Request.InputStream);
                var userName = await reader.ReadToEndAsync();
                var greeting = "Hello " + userName;

                await ctx.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(greeting));
                ctx.Response.Close();
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
    public async Task<string> Http()
    {
        var http = new HttpClient();
        var response = await http.PostAsync("http://localhost:" + HttpPort, new StringContent(UserName));
        return await response.Content.ReadAsStringAsync();
    }

    [Benchmark]
    public async Task<string> Grpc()
    {
        var channel = new Channel("127.0.0.1:" + GrpcPort, ChannelCredentials.Insecure);

        var client = new Greeter.GreeterClient(channel);
        var reply = await client.SayHelloAsync(new HelloRequest { Name = UserName });

        await channel.ShutdownAsync();

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
