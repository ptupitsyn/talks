using StackExchange.Redis;

namespace ImplementingDbDriver;

public static class RedisQuery
{
    public static void Run()
    {
        using ConnectionMultiplexer connection = ConnectionMultiplexer.Connect("127.0.0.1");
        IDatabase db = connection.GetDatabase()!;
        db.StringGet("1");
    }
}
