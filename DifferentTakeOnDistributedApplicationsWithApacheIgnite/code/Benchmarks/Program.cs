using System;
using BenchmarkDotNet.Running;
using StackExchange.Redis;

namespace Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            // ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("127.0.0.1");
            //
            // var db = redis.GetDatabase();
            // db.StringSet("k", "v");

            BenchmarkRunner.Run<CacheGetRedisIgnite>();
        }
    }
}
