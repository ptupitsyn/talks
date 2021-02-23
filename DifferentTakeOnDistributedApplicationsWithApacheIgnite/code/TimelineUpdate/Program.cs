using System;
using System.Collections.Generic;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Affinity;

// TODO: Use separate program to demonstrate colocated updates?
using var ignite = Ignition.Start();

var idGen = ignite.GetAtomicSequence(name: "id", initialValue: 0, create: true);
var users = ignite.GetOrCreateCache<UserKey, User>("user");
var posts = ignite.GetOrCreateCache<PostKey, Post>("post");
var followers = ignite.GetOrCreateCache<UserKey, IList<UserKey>>("follower");
var timelines = ignite.GetOrCreateCache<UserKey, IList<PostKey>>("timeline");


void NewPost(int userId, string text)
{
    var userKey = new UserKey(userId);
    var postKey = new PostKey(idGen.Increment(), userKey);

    // Create the post.
    posts.Put(postKey, new Post(text));

    // Add post to user's public timeline
    // Assume we are on the primary already

    // Add post to every follower's timeline
    var userFollowers = followers.TryGet(userKey, out var res) ? res : Array.Empty<UserKey>();

    if (userFollowers.Count > 0)
    {
        // Send updates to primary nodes for every follower.
        timelines.InvokeAll(userFollowers, new TimelineUpdater(), postKey);
    }
}

record User(string Name);

record UserKey(int Id);

record Post(string Text);

record PostKey(
    long Id,
    [property:AffinityKeyMapped] UserKey UserId);

record Timeline(long UserId, IList<PostKey> PostKeys);

class TimelineUpdater : ICacheEntryProcessor<UserKey, IList<PostKey>, PostKey, object>
{
    public object Process(IMutableCacheEntry<UserKey, IList<PostKey>> entry, PostKey arg)
    {
        entry.Value.Add(arg);
        entry.Value = entry.Value; // Force update

        return null;
    }
}
