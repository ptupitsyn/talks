using System.Collections.Concurrent;

namespace ImplementingDbDriver;

public class RequestHeader
{
    private readonly ConcurrentDictionary<long, TaskCompletionSource> _requests = new();
    private long _requestId;

    public Task SendAsync()
    {
        var tcs = new TaskCompletionSource();
        _requests[Interlocked.Increment(ref _requestId)] = tcs;
        return tcs.Task;
    }

    private async Task ReceiveLoop()
    {
        while (true)
        {
            var msg = await ReceiveNextAsync();
            if (_requests.TryRemove(msg.Id, out var tcs))
                tcs.SetResult();
        }
    }

    private async Task<(long Id, byte[] Payload)> ReceiveNextAsync()
    {
        await Task.Yield();

        return (default, default!);
    }
}
