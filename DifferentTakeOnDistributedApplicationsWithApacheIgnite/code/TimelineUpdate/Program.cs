using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache.Affinity;

// TODO: Use separate program to demonstrate colocated updates
using var ignite = Ignition.Start();

record User(string Name);

record Post(string Text);

record PostKey(
    int PostId,
    [property:AffinityKeyMapped] int UserId);

