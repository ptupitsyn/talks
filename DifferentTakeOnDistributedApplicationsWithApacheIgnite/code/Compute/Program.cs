using System;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache.Affinity.Rendezvous;
using Apache.Ignite.Core.Cache.Query;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Log;

// Run this multiple times to create the distributed environment.
using var ignite = Ignition.Start(new IgniteConfiguration
{
    Logger = new ConsoleLogger {MinLevel = LogLevel.Error},
    JvmOptions = new[]{"-DIGNITE_QUIET=false"}
});

