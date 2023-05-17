// See https://aka.ms/new-console-template for more information
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Data;




using var ctx = new MyContext();

// access with transport object and row name
var result = await ctx.RawSqlQuery($"select name from table where id=1", x => new StringResult { Name = $"{x["name"]}" });
Console.WriteLine(result.FirstOrDefault());

// alternative with object mapping  and row name
var result2 = await ctx.RawSqlQuery($"select name from table where id=1", x => x["name"]);
Console.WriteLine(result.FirstOrDefault());

// alternative with object mapping  and row index
var result3 = await ctx.RawSqlQuery($"select name from table where id=1", x => x[0]);
Console.WriteLine(result.FirstOrDefault());

/// <summary>
/// transport
/// </summary>
class StringResult
{
    public string? Name { get; set; }
}

/// <summary>
/// db context
/// </summary>
class MyContext : DbContext
{
    private const string _connectionString = "";


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(_connectionString);
    }
}

/// <summary>
/// extension class
/// </summary>
public static class DbContextExtensions
{
    // credits to: https://stackoverflow.com/a/43892614/6404726
    public static async Task<List<T>> RawSqlQuery<T>(this DbContext context, string query, Func<DbDataReader, T> map)
    {
        using var command = context.Database.GetDbConnection().CreateCommand();

        command.CommandText = query;
        command.CommandType = CommandType.Text;

        context.Database.OpenConnection();

        using (var result = await command.ExecuteReaderAsync())
        {
            var entities = new List<T>();

            while (await result.ReadAsync())
            {
                entities.Add(map(result));
            }

            return entities;
        }

    }
}

