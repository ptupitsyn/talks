using System;
using System.Collections.Generic;
using System.Linq;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Affinity;

// Run this multiple times to create the distributed environment.
using var ignite = Ignition.Start();

var idGen = ignite.GetAtomicSequence(name: "id", initialValue: 0, create: true);
var users = ignite.GetOrCreateCache<UserKey, User>("user");
var posts = ignite.GetOrCreateCache<PostKey, Post>("post");
var followers = ignite.GetOrCreateCache<UserKey, IList<UserKey>>("follower");
var timelines = ignite.GetOrCreateCache<UserKey, IList<PostKey>>("timeline");

// Create users.
var bloggerId = new UserKey(idGen.Increment());
var followerIds = Enumerable.Range(1, 10).Select(_ => new UserKey(idGen.Increment())).ToList();
followers.Put(bloggerId, followerIds);

// Create new post.
Console.ReadKey();
NewPost(bloggerId, "Hello, World!");

void NewPost(UserKey userKey, string text)
{
    // Create the post.
    var postKey = new PostKey(idGen.Increment(), userKey);
    posts.Put(postKey, new Post(text));

    // Add post to every follower's timeline.
    var userFollowers = followers.TryGet(userKey, out var res) ? res : Array.Empty<UserKey>();

    if (userFollowers.Count > 0)
    {
        // Send updates to primary nodes for every follower.
        timelines.InvokeAll(userFollowers, new TimelineUpdater(), postKey);
    }
}

record User(string Name);

record UserKey(long Id);

record Post(string Text);

record PostKey(
    long Id,
    [property:AffinityKeyMapped] UserKey UserId);

record Timeline(long UserId, IList<PostKey> PostKeys);

class TimelineUpdater : ICacheEntryProcessor<UserKey, IList<PostKey>, PostKey, object>
{
    public object Process(IMutableCacheEntry<UserKey, IList<PostKey>> entry, PostKey arg)
    {
        Console.WriteLine($">>> Updating timeline for user {entry.Key} with post {arg}...");

        var posts = entry.Exists ? entry.Value : new List<PostKey>();
        posts.Add(arg);
        entry.Value = posts;

        return null;
    }
}
