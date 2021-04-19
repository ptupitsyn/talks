using System;
using System.Linq;
using System.Threading;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache.Affinity;
using Apache.Ignite.Core.Log;

var cfg = new IgniteConfiguration
{
    AutoGenerateIgniteInstanceName = true,
    Logger = new ConsoleLogger{MinLevel = LogLevel.Error},
    // JvmOptions = new[]{"-DIGNITE_QUIET=false"}
};

using var ignite1 = Ignition.Start(cfg);
using var ignite2 = Ignition.Start(cfg);

// Same cache referenced from different nodes.
var accountsCache1 = ignite1.GetOrCreateCache<int, User>("user");
var accountsCache2 = ignite2.GetOrCreateCache<int, User>("user");

var postsCache1 = ignite1.GetOrCreateCache<PostKey, Post>("post");
var postsCache2 = ignite2.GetOrCreateCache<PostKey, Post>("post");

var userId = 0;
accountsCache1[userId] = new ("Ivan");

for (int i = 0; i < 10; i++)
    postsCache1[new(i, userId)] = new ("Text");

Thread.Sleep(200); // Wait for rebalance

Console.WriteLine(">>> Node 1 posts: " + postsCache1.GetLocalSize());
Console.WriteLine(">>> Node 2 posts: " + postsCache2.GetLocalSize());
Console.WriteLine(">>> Node 1 accounts: " + accountsCache1.GetLocalSize());
Console.WriteLine(">>> Node 2 accounts: " + accountsCache2.GetLocalSize());

Console.ReadKey();


record User(string Name);

record Post(string Text);

record PostKey(
    int PostId,
    [property:AffinityKeyMapped] int UserId);
