using System;
using System.Diagnostics;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Client;
using Apache.Ignite.Core.Log;

var stopwatch = Stopwatch.StartNew();

var clientConfiguration = new IgniteClientConfiguration("127.0.0.1:10800")
{
    Logger = new ConsoleLogger { MinLevel = LogLevel.Debug }
};

using var client = Ignition.StartClient(clientConfiguration);

Console.WriteLine($"Client connected in {stopwatch.Elapsed}.");

var cache = client.CreateCache<Guid, Car>("cars");

const int count = 100_000;
stopwatch.Restart();

using (var streamer = client.GetDataStreamer<Guid, Car>(cache.Name))
{
    for (int i = 0; i < count; i++)
    {
        streamer.Add(Guid.NewGuid(), new Car($"abc-{i}", i));
    }
}

Console.WriteLine($"Streamed {count} entries in {stopwatch.Elapsed}.");

record Car(string License, int Mileage);
