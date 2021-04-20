using System;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Log;

using IIgnite ignite = Ignition.Start(new IgniteConfiguration
{
    Logger = new ConsoleLogger {MinLevel = LogLevel.Error},
    JvmOptions = new[]{"-DIGNITE_QUIET=false"}
});

var cfg = new CacheConfiguration
{
    Name = "person",
    CacheMode = CacheMode.Replicated,
    PlatformCacheConfiguration = new PlatformCacheConfiguration()
};

ICache<int, Person> cache = ignite.GetOrCreateCache<int, Person>(cfg);
cache.PutIfAbsent(1, new Person("Ivan", 29));

Console.WriteLine(">>> Value written to cache");
Console.ReadKey();

public record Person(string Name, int Age);
