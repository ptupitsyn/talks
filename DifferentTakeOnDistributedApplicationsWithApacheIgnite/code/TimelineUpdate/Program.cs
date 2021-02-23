using System;
using System.Collections.Generic;
using System.Linq;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Affinity;

// TODO: Use separate program to demonstrate colocated updates?
using var ignite = Ignition.Start();

record User(string Name);

record Post(string Text);

record PostKey(
    int PostId,
    [property:AffinityKeyMapped] int UserId);

record Timeline(int UserId, IReadOnlyCollection<PostKey> PostKeys);

class TimelineUpdater : ICacheEntryProcessor<int, Timeline, PostKey, object>
{
    public object Process(IMutableCacheEntry<int, Timeline> entry, PostKey arg)
    {
        entry.Value = entry.Value with
        {
            PostKeys = new List<PostKey>(entry.Value.PostKeys) {arg}
        };

        return null;
    }
}
