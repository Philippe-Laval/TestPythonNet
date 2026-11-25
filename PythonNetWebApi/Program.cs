
using Python.Runtime;
using PythonNetWebApi.Models;
using PythonNetWebApi.Services;

namespace PythonNetWebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            PythonInitializer.InitializePython();

            // Add services to the container.

            // Enregistrement en Singleton
            builder.Services.AddSingleton<ISqlGlotService, SqlGlotService>();
            builder.Services.AddSingleton<IOtherPythonService, OtherPythonService>();


            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Optional: Warm up services on startup
            app.Services.GetRequiredService<ISqlGlotService>();
            app.Services.GetRequiredService<IOtherPythonService>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            // Endpoint Minimal API de démonstration
            app.MapPost("/api/sql/transpile", (TranspileRequest req, ISqlGlotService sqlGlot) =>
            {
                try
                {
                    string converted = sqlGlot.Transpile(req.Sql, req.FromDialect, req.ToDialect);
                    return Results.Ok(new { Sql = converted });
                }
                catch (PythonException ex)
                {
                    // Capture proprement les erreurs de syntaxe SQL (sqlglot.errors.ParseError)
                    return Results.BadRequest(new { Error = ex.Message });
                }
            });

            app.Run();
        }
    }
}
