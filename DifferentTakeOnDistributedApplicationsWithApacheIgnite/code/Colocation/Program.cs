﻿using System;
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

// Start without colocation and show the problem, then fix it.
var accountsCache = ignite1.GetOrCreateCache<int, Account>("account");

// Same cache referenced from different nodes.
var postsCache1 = ignite1.GetOrCreateCache<PostKey, Post>("post");
var postsCache2 = ignite2.GetOrCreateCache<PostKey, Post>("post");

var accountId = 0;
accountsCache[accountId] = new ("Ivan");

for (int i = 0; i < 10; i++)
    postsCache1[new(i, accountId)] = new ("Text", accountId);

Thread.Sleep(200); // Wait for rebalance

Console.WriteLine(">>> Node 1: " + postsCache1.GetLocalEntries().Count());
Console.WriteLine(">>> Node 2: " + postsCache2.GetLocalEntries().Count());

Console.ReadKey();


record Account(string Name);

record Post(string Text, int AccountId);

record PostKey(
    int PostId,
    [property:AffinityKeyMapped] int AccountId);