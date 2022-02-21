using Npgsql;

namespace ImplementingDbDriver;

public static class PgQuery
{
    public static void Run()
    {
        using var conn = new NpgsqlConnection("db-host");
        conn.Open();

        using var cmd = new NpgsqlCommand("SELECT Name FROM User WHERE CompanyId = @cid", conn)
        {
            Parameters = { new("cid", 123L) }
        };

        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            Console.WriteLine(reader.GetString(0));
        }
    }
}
