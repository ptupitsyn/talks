using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using BenchmarkDotNet.Attributes;
using Dapr.Client;
using StackExchange.Redis;

namespace Benchmarks
{
    /// <summary>
    /// |           Method |         Mean |      Error |     StdDev |    Ratio | RatioSD |
    /// |----------------- |-------------:|-----------:|-----------:|---------:|--------:|
    /// | GetIgniteClrHeap |     46.33 ns |   0.305 ns |   0.286 ns |     1.00 |    0.00 |
    /// |        GetIgnite |  1,929.56 ns |  36.238 ns |  33.897 ns |    41.65 |    0.84 |
    /// |         GetRedis | 60,080.39 ns | 536.235 ns | 475.358 ns | 1,296.69 |   11.13 |
    /// </summary>
    public class CacheGetRedisIgnite
    {
        private IDatabase _redis;

        private ICache<string, string> _ignite;

        private ICache<string, string> _igniteClr;

        private DaprClient _dapr;

        [GlobalSetup]
        public void GlobalSetup()
        {
            // In-process Ignite server node.
            var ignite = Ignition.Start();

            // In-process cache (unmanaged memory + optional persistence).
            _ignite = ignite.GetOrCreateCache<string, string>(new CacheConfiguration
            {
                Name = "cache"
            });

            // In-process cache (unmanaged memory + optional persistence) with a CLR heap layer.
            _igniteClr = ignite.GetOrCreateCache<string, string>(new CacheConfiguration
            {
                Name = "cache-with-clr-heap",
                PlatformCacheConfiguration = new PlatformCacheConfiguration()
            });

            // Redis: docker run -d -p 6379:6379 redis
            _redis = ConnectionMultiplexer.Connect("127.0.0.1").GetDatabase();

            // Dapr: dapr run dotnet run -c Release
            _dapr = new DaprClientBuilder().Build();
            _dapr.SaveStateAsync("statestore", "1", "2").GetAwaiter().GetResult();

            // Default data.
            _ignite.Put("1", "2");
            _igniteClr.Put("1", "2");
            _redis.StringSet("1", "2");
        }

        [Benchmark(Baseline = true)] public void GetIgniteClrHeap() => _igniteClr.Get("1");

        [Benchmark] public void GetIgnite() => _ignite.Get("1");

        [Benchmark] public void GetRedis() => _redis.StringGet("1");

        [Benchmark] public void GetDapr() => _dapr.GetStateAsync<string>("statestore", "1");
    }
}
