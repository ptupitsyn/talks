using Npgsql;

using var conn = new NpgsqlConnection("db-host");
conn.Open();

using var cmd = new NpgsqlCommand("SELECT Name FROM User", conn);
using var reader = cmd.ExecuteReader();

while (reader.Read())
{
    Console.WriteLine(reader.GetString(0));
}
