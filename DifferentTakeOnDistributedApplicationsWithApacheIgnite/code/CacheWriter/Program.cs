using System;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache.Configuration;

using var ignite = Ignition.Start();

var cfg = new CacheConfiguration
{
    Name = "person",
    CacheMode = CacheMode.Replicated
};

var cache = ignite.GetOrCreateCache<int, Person>(cfg);
cache.PutIfAbsent(1, new Person("Ivan", 29));

Console.WriteLine(">>> Value written to cache");
Console.ReadKey();

public record Person(string Name, int Age);
