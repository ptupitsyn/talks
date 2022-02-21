using StackExchange.Redis;

namespace ImplementingDbDriver;

public static class RedisQuery
{
    public static void Run()
    {
        using var multiplexer = ConnectionMultiplexer.Connect("127.0.0.1");
        IDatabase db = multiplexer.GetDatabase()!;
        db.StringGet("1");

        var channel = multiplexer.GetSubscriber().Subscribe("messages");
        channel.OnMessage(message => Console.WriteLine((string)message.Message));
    }
}
