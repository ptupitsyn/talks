using System.Collections.Concurrent;

namespace ImplementingDbDriver;

public class RequestHeader
{
    private const byte MessageTypeResponse = 1;
    private const byte MessageTypeNotification = 2;

    private readonly ConcurrentDictionary<long, TaskCompletionSource<byte[]>> _requests = new();
    private readonly ConcurrentDictionary<long, Action<byte[]>> _listeners = new();
    private long _requestId;

    public Task SendAsync()
    {
        var tcs = new TaskCompletionSource<byte[]>();
        _requests[Interlocked.Increment(ref _requestId)] = tcs;
        return tcs.Task;
    }

    private async Task ReceiveLoop()
    {
        while (true)
        {
            var msg = await ReceiveNextAsync();
            if (msg.Type == MessageTypeResponse)
            {
                if (_requests.TryRemove(msg.Id, out var tcs))
                    tcs.SetResult(msg.Payload);
            }
            else if (msg.Type == MessageTypeNotification)
            {
                if (_listeners.TryRemove(msg.Id, out var listener))
                    listener.Invoke(msg.Payload);
            }
        }
    }

    private async Task<(long Id, byte Type, byte[] Payload)> ReceiveNextAsync()
    {
        await Task.Yield();

        return (default, default, default!);
    }
}
