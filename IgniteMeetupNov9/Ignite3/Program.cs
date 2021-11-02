using System;
using System.Collections.Generic;
using Apache.Ignite;
using Apache.Ignite.Log;
using Apache.Ignite.Table;

// Connect.
var cfg = new IgniteClientConfiguration("127.0.0.1:10800")
{
    Logger = new ConsoleLogger { MinLevel = LogLevel.Debug }
};

using var client = await IgniteClient.StartAsync(cfg);

// Print table names.
IList<ITable> tables = await client.Tables.GetTablesAsync();

foreach (var t in tables)
    Console.WriteLine(t.Name);

// Get table by name.
ITableView<IIgniteTuple> table = await client.Tables.GetTableAsync("PUBLIC.accounts");
Console.WriteLine("Table exists: " + (table != null));

// Insert row.
var row = new IgniteTuple
{
    ["accountNumber"] = 101,
    ["balance"] = (double)300,
    ["firstName"] = "First",
    ["lastName"] = "Last"
};

await table.UpsertAsync(row);

// Reuse row instance.
row["accountNumber"] = 103;
row["balance"] = (double)400;
row["firstName"] = "X";
row["firstName"] = "Y";

await table.UpsertAsync(row);

// Get row by key.
var key = new IgniteTuple();
key["accountNumber"] = 101;

IIgniteTuple val = await table.GetAsync(key);
Console.WriteLine(val);
