using System;
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
var accounts1 = ignite1.GetOrCreateCache<Guid, Account>("account");
var posts1 = ignite1.GetOrCreateCache<Guid, Post>("post");


record Account(string Name);

record Post([property:AffinityKeyMapped]string Text);

record PostKey(
    Guid PostId,
    [property:AffinityKeyMapped] Guid AccountId);
