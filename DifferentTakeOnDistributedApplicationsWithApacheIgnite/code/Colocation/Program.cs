using System;
using System.Linq;
using System.Threading;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache.Affinity;
using Apache.Ignite.Core.Log;

var cfg = new IgniteConfiguration
{
    AutoGenerateIgniteInstanceName = true,
    Logger = new ConsoleLogger{MinLevel = LogLevel.Error},
};

