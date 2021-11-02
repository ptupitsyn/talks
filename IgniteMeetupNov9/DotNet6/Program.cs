using System;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Log;

using var ignite = Ignition.Start(new IgniteConfiguration
{
    Logger = new ConsoleLogger {MinLevel = LogLevel.Error},
});

var cache = ignite.GetOrCreateCache<EmployeeKey, Employee>("employee");

EmployeeKey key1 = new(2, "b-1");
EmployeeKey key2 = new(2, "b-1");

cache.Put(key1, new("John Doe", new DateTime(2016, 1, 1)));

Console.WriteLine();
Console.WriteLine($">>> Value from cache: {cache.Get(key2)}");
Console.WriteLine($">>> Equals: {key1 == key2}, ReferenceEquals: {ReferenceEquals(key1, key2)}");
Console.WriteLine();

public sealed record Employee(string Name, DateTime StartDate);

public sealed record EmployeeKey(int CompanyId, string Id);

// dotnet publish --runtime linux-x64 /p:PublishSingleFile=true /p:IncludeAllContentForSelfExtract=true --self-contained true --output pub
