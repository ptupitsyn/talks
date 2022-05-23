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

        // Raw socket.
    }

    [Benchmark]
    public async Task RawSocket()
    {
        // TODO
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
        var reply = await client.SayHelloAsync(new HelloRequest { Name = "John Doe" });

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
