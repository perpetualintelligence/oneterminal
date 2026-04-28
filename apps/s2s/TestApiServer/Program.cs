using OneImlx.Shared.Licensing;
using OneImlx.Terminal.Apps.TestApiServer.Runners;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Hosting;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Server;
using OneImlx.Terminal.Server.Extensions;
using OneImlx.Terminal.Shared;
using OneImlx.Terminal.Stores;
using System.Net;
using System.Text;

namespace OneImlx.Terminal.Apps.TestApiServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


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
            terminalBuilder.AddDeclarativeAssembly<TestApiServerRunner>();

            var app = builder.Build();


            app.UseHttpsRedirection();

            // Sample .NET API endpoint
            app.MapGet("/api/pingdotnet", (HttpRequest request, IServiceProvider services) =>
            {
                ITerminalConsole terminalConsole = services.GetRequiredService<ITerminalConsole>();
                terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "ASP.NET API: Ping /api/pingdotnet").Wait();

                return Results.Ok(new { request = request.Path.Value, timestamp = DateTime.UtcNow, status="OK" });
            });

            // Sample .NET API endpoint to get status from terminal services
            app.MapGet("/api/pingterminal", (IServiceProvider services) =>
            {
                var router = services.GetRequiredService<ITerminalRouter<TerminalHttpRouterContext>>();
                var processor = services.GetRequiredService<ITerminalProcessor>();
                ITerminalConsole terminalConsole = services.GetRequiredService<ITerminalConsole>();
                terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "ASP.NET API: Ping /api/pingterminal").Wait();

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
            app.RunTerminalRouterBackground<TerminalHttpRouter, TerminalHttpRouterContext>(context);

            // Run both API server and Terminal HTTP router concurrently
            app.Run();
        }
    }
}