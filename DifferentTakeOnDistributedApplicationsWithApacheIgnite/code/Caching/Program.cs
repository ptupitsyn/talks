using System;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;

using var ignite = Ignition.Start();

var cfg = new CacheConfiguration
{
    Name = "person",
    CacheMode = CacheMode.Replicated,
    PlatformCacheConfiguration = new PlatformCacheConfiguration()
};

var cache = ignite.GetOrCreateCache<int, Person>(cfg);

if (!cache.PutIfAbsent(1, new Person("Ivan", 29)))
{
    var res = cache.LocalPeek(1, CachePeekMode.Platform);
    Console.WriteLine("Value from cache: " + res);
}
else
{
    Console.WriteLine("Value written to cache");
}

public record Person(string Name, int Age);
