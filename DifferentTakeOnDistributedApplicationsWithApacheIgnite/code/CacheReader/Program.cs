using System;
using Apache.Ignite.Core;

using var ignite = Ignition.Start();

var cache = ignite.GetCache<int, Person>("person");

Console.WriteLine(">>>>> Value from cache: " + cache.Get(1));
Console.ReadKey();

public record Person(string Name, int Age);
