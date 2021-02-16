using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using BenchmarkDotNet.Attributes;
using StackExchange.Redis;

namespace Benchmarks
{
    public class CacheGetRedisIgnite
    {
        private IDatabase _redis;

        private ICache<string, string> _ignite;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _ignite = Ignition.Start().GetOrCreateCache<string, string>("c");
            _ignite.Put("1", "2");

            _redis = ConnectionMultiplexer.Connect("127.0.0.1").GetDatabase();
            _redis.StringSet("1", "2");
        }

        [Benchmark]
        public void GetIgnite()
        {
            _ignite.Get("1");
        }

        [Benchmark]
        public void GetRedis()
        {
            _redis.StringGet("1");
        }
    }
}
