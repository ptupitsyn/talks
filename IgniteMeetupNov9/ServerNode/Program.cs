using System;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Log;

using var ignite = Ignition.Start(new IgniteConfiguration
{
    Logger = new ConsoleLogger {MinLevel = LogLevel.Error},
});

ignite.GetOrCreateCache<object, object>("cars");

Console.WriteLine(">>> Started!");

Console.ReadKey();
