using System;
using System.Linq;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache.Affinity;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Log;

var cfg = new IgniteConfiguration
{
    AutoGenerateIgniteInstanceName = true,
    Logger = new ConsoleLogger{MinLevel = LogLevel.Error}
};

using var ignite1 = Ignition.Start(cfg);
using var ignite2 = Ignition.Start(cfg);

// Start without colocation and show the problem, then fix it.
var accountsCache1 = ignite1.GetOrCreateCache<int, Account>("account");
var postsCache1 = ignite1.GetOrCreateCache<int, Post>("post");

var accountsCache2 = ignite1.GetOrCreateCache<int, Account>("account");
var postsCache2 = ignite1.GetOrCreateCache<int, Post>("post");

accountsCache1[1] = new Account("Ivan");

for (int i = 0; i < 10; i++)
    postsCache1[0] = new Post("Text", 1);


record Account(string Name);

record Post(string Text, int AccountId);

record PostKey(
    int PostId,
    [property:AffinityKeyMapped] int AccountId);
