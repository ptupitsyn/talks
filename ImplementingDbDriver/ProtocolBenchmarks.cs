using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace ImplementingDbDriver;

/// <summary>
/// Compares "Hello World" exchange using HTTP, gRPC and raw Socket APIs.
/// </summary>
public class ProtocolBenchmarks
{
    public static void Run()
    {
        BenchmarkRunner.Run<ProtocolBenchmarks>();
    }

    [Benchmark]
    public void RawSocket()
    {
        // TODO
    }

    [Benchmark]
    public void Http()
    {
        // TODO

    }

    [Benchmark]
    public void Grpc()
    {
        // TODO

    }
}
