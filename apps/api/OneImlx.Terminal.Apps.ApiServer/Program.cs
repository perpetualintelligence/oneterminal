using OneImlx.Shared.Licensing;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Hosting;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Server;
using OneImlx.Terminal.Server.Extensions;
using OneImlx.Terminal.Shared;
using OneImlx.Terminal.Stores;
using System;
using System.Net;
using System.Text;

namespace OneImlx.Terminal.Apps.TestApiServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            // Configure Terminal Framework
            builder.Services.AddHostedService<TestApiServerHostedService>();
            builder.Services.AddScoped<TerminalHttpMapService>();
            var textHandler = new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.UTF8);
            ITerminalBuilder terminalBuilder = builder.Services.AddTerminalConsole<TerminalInMemoryCommandStore, TerminalConsoleHelpProvider, TerminalConsoleExceptionHandler, TerminalSystemConsole>(
                new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII),
                options =>
                {
                    options.Id = TerminalIdentifiers.TestApplicationId; // Set the application ID.
                    options.Licensing.LicenseFile = "C:\\this\\lic\\oneimlx-terminal-demo-test.json"; // License file path.
                    options.Licensing.LicensePlan = ProductCatalog.TerminalPlanDemo; // License plan to use (Demo in this case).
                    options.Router.MaxLength = 64000; // Set max length for remote messages.
                    options.Router.Caret = "> "; // Caret for the terminal.
                });

            terminalBuilder.AddTerminalRouter<TerminalHttpRouter, TerminalHttpRouterContext>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();

            // Test endpoint - Standard API
            app.MapGet("/api/pingdotnet", () => Results.Ok(new { message = "API Server is running", timestamp = DateTime.UtcNow }));

            // Test endpoint - Terminal Router Status
            app.MapGet("/api/pingterminal", (IServiceProvider services) =>
            {
                var router = services.GetService<ITerminalRouter<TerminalHttpRouterContext>>();
                var processor = services.GetService<ITerminalProcessor>();

                return Results.Ok(new
                {
                    routerRunning = router?.IsRunning ?? false,
                    processorRunning = processor?.IsProcessing ?? false,
                    timestamp = DateTime.UtcNow
                });
            });

            // Map Terminal HTTP Endpoint - POST /oneimlx/terminal/httprouter
            app.MapTerminalHttp();

            // Initialize Terminal HTTP Router
            var endpoint = new IPEndPoint(IPAddress.Loopback, 0);
            var context = new TerminalHttpRouterContext(endpoint, TerminalStartMode.Http, null, null);
            app.RunTerminalRouterBackgroundAsync<TerminalHttpRouter, TerminalHttpRouterContext>(context);

            // Run both API server and Terminal HTTP router concurrently
            app.Run();
        }
    }
}