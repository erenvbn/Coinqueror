using Microsoft.AspNetCore.Mvc;
using Npgsql;

[Route("api/[controller]")]
[ApiController]
public class TestController : ControllerBase
{
    [HttpGet("Test", Name = "Test")]
    public IActionResult Test()
    {
        //"Server=/cloudsql/your-project-id:us-central1:instance-name;Uid=aspnetuser;Pwd=;Database=votes"

        //Project id
        //useful-figure-437910-t5

        //Public id:34.32.74.38
        //Outgoing IP: 34.32.31.45
        //Connection name
        //useful-figure-437910-t5:europe-west10:coinqueror-user-postgres

        //"connectionstrings": {
        //    "mypostgresconnection": "host=34.32.74.38;port=5432;database=postgres;username=postgres;password=e1r2e3n*;sslmode=require;trustservercertificate=true";

        var connectionString = NewPostgreSqlTCPConnectionString().ConnectionString;
        //var connectionString = "Host=localhost;Port=5432;Database=coinquerorDb;Username=postgres;Password=1;SslMode=Require;TrustServerCertificate=True";

        try
        {
            using (var myConnection = new NpgsqlConnection(connectionString))
            {
                myConnection.Open();
                Console.WriteLine("CONNECTION OPENED");
                Console.WriteLine("CONNECTION STRING: " + connectionString);
                Console.WriteLine(myConnection.UserName);

                using (var myCommand = new NpgsqlCommand("SELECT * FROM public.users", myConnection))
                {
                    using (var reader = myCommand.ExecuteReader())
                    {
                        var users = new List<object>();

                        while (reader.Read())
                        {
                            var user = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                user[reader.GetName(i)] = reader.GetValue(i);
                            }
                            users.Add(user);
                        }
                        return Ok(users);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Database connection failed: {ex.Message}");
        }
    }

    public static NpgsqlConnectionStringBuilder NewPostgreSqlTCPConnectionString()
    {
        var connectionString = new NpgsqlConnectionStringBuilder()
        {
            //Host = "34.32.74.38",
            //Port = 5432,
            //Username = "postgres",
            //Password = "E1r2e3n*", // Replace with your password
            //Database = "postgres",
            //SslMode = SslMode.Disable // Adjust if SSL is required
            Host = "localhost",
            Port = 5432,
            Username = "postgres",
            Password = "1", // Replace with your password
            Database = "coinquerorDb",
            SslMode = SslMode.Disable // Adjust if SSL is required
        };
        connectionString.Pooling = true;
        return connectionString;
    }
}


//"Server=/cloudsql/your-project-id:us-central1:instance-name;Uid=aspnetuser;Pwd=;Database=votes"