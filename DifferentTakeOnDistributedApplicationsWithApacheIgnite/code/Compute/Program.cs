using System;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache.Affinity.Rendezvous;
using Apache.Ignite.Core.Cache.Query;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Log;

// Run this multiple times to create the distributed environment.
using var ignite = Ignition.Start(new IgniteConfiguration
{
    Logger = new ConsoleLogger {MinLevel = LogLevel.Error},
    JvmOptions = new[]{"-DIGNITE_QUIET=false"}
});

// Create caches (tables).
var posts = ignite.GetOrCreateCache<long, Post>("post");
for (int i = 0; i < 10; i++)
{
    posts[i] = new Post($"Foo {i} bar", false);
}

// Scan everything by partition.
Console.WriteLine("Ready...");
Console.ReadKey();

for (int part = 0; part < RendezvousAffinityFunction.DefaultPartitions; part++)
{
    // AffinityRun locks the partition in place and guarantees that all data is available locally during execution.
    ignite.GetCompute().AffinityRun(cacheNames: new[]{posts.Name}, partition: part, action: new Scanner(part));
}

Console.ReadKey();

record Post(string Text, bool Banned);

class Scanner : IComputeAction
{
    public Scanner(int partition)
    {
        Partition = partition;
    }

    public int Partition { get; }

    public void Invoke()
    {
        var cache = Ignition.GetIgnite().GetCache<long, Post>("post");

        // Scan specified partition.
        // All operations here are guaranteed to be local and don't involve any network calls.
        var query = new ScanQuery<long, Post>
        {
            Partition = Partition
        };

        foreach (var cacheEntry in cache.Query(query))
        {
            if (cacheEntry.Value.Text.Contains("7"))
            {
                // Digit 7 is banned today.
                cache.Put(cacheEntry.Key, cacheEntry.Value with {Banned = true});
            }

            Console.WriteLine($"Entry processed: {cacheEntry.Key}");
        }
    }
}
