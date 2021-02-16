using System;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Binary;

using var ignite = Ignition.Start();

var cache = ignite.GetCache<int, Person>("person");

// Console.WriteLine(">>>>> Value from cache: " + cache.Get(1));
Console.ReadKey();
Console.WriteLine("\n>>>> Same instance: " + ReferenceEquals(cache.Get(1), cache.Get(1)));
Console.ReadKey();

//public record Person(string Name, int Age);

public class Person : IBinarizable
{
    public int Age { get; set; }

    public string Name { get; set; }

    public void WriteBinary(IBinaryWriter writer)
    {
        // No-op.
    }

    public void ReadBinary(IBinaryReader reader)
    {
        Name = reader.ReadString("Name");
        Age = reader.ReadInt("Age");

        Console.WriteLine("\n>>>> Value Deserialized: " + Name);
    }
}
