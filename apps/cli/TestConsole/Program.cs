using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OneImlx.Shared.Licensing;
using OneImlx.Terminal.Apps.Test.Runners;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Hosting;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;
using OneImlx.Terminal.Stores;
using Serilog;

namespace OneImlx.Terminal.Apps.Test
{
    internal class Program
    {
        private static void ConfigureAppConfigurationDelegate(HostBuilderContext context, IConfigurationBuilder builder)
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"), optional: false, reloadOnChange: false);
            configBuilder.Build();
        }

        private static void ConfigureLoggingDelegate(HostBuilderContext context, ILoggingBuilder builder)
        {
            // Clear all providers
            builder.ClearProviders();

            // Configure logging of your choice, here we are configuring Serilog
            var loggerConfig = new LoggerConfiguration();
            loggerConfig.MinimumLevel.Error();
            loggerConfig.WriteTo.Console();
            Log.Logger = loggerConfig.CreateLogger();
            builder.AddSerilog(Log.Logger);
        }

        private static void ConfigureOneImlxTerminal(IServiceCollection collection)
        {
            // Configure the hosted service
            collection.AddHostedService<TestAppHostedService>();

            // NOTE: Specify your demo or commercial license file. Specify your application id.
            TerminalTextHandler textHandler = new(StringComparison.OrdinalIgnoreCase, Encoding.Unicode);
            ITerminalBuilder terminalBuilder = collection.AddTerminalConsole<TerminalInMemoryCommandStore>(textHandler,
                options =>
                {
                    options.Id = TerminalIdentifiers.TestApplicationId;

                    options.Driver.Enabled = true;
                    options.Driver.RootId = "test";

                    options.Licensing.LicenseFile = "C:\\this\\lic\\oneimlx-terminal-offline-test.json";
                    options.Licensing.LicensePlan = ProductCatalog.TerminalPlanCorporate;
                    options.Licensing.Deployment = TerminalIdentifiers.AirGappedDeployment;

                    options.Router.Caret = "> ";
                }
                                                                                                                                                                                               );

            // You can use declarative or explicit syntax. Here we are using declarative syntax.
            {
                // Add commands using declarative syntax.
                terminalBuilder.AddDeclarativeAssembly<TestRunner>();

                // OR

                // Add commands using explicit syntax.
                //RegisterCommands(terminalBuilder);
            }
        }

        private static void ConfigureServicesDelegate(HostBuilderContext context, IServiceCollection services)
        {
            // Disable hosting status message
            services.Configure<ConsoleLifetimeOptions>(static options =>
            {
                options.SuppressStatusMessages = true;
            });

            // Configure OneImlx.Terminal services
            ConfigureOneImlxTerminal(services);

            // Configure other services
        }

        private static bool IsNewTerminal()
        {
            try
            {
                int parentPid = -1;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Windows: Use WMI to fetch ParentProcessId
                    using var searcher = new System.Management.ManagementObjectSearcher(
                        $"SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {Environment.ProcessId}");
                    foreach (var obj in searcher.Get())
                    {
                        parentPid = Convert.ToInt32(obj["ParentProcessId"]);
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                         RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    // Linux / macOS: Use 'ps' command to get parent PID
                    using var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "sh",
                            Arguments = $"-c \"ps -o ppid= -p {Process.GetCurrentProcess().Id}\"",
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    if (int.TryParse(output.Trim(), out int parsedPid))
                    {
                        parentPid = parsedPid;
                    }
                }

                if (parentPid > 0)
                {
                    using var parentProcess = Process.GetProcessById(parentPid);
                    string parentName = parentProcess.ProcessName.ToLowerInvariant();

                    // Handle specific known console/debug parents
                    return parentName switch
                    {
                        "vsdebugconsole" => true,       // Visual Studio Debug Console
                        "explorer" => true,             // Windows File Explorer
                        "finder" => true,               // macOS Finder
                        "nautilus" => true,             // Linux Nautilus (File Explorer)
                        _ => false                      // Default to terminal
                    };
                }

                return false; // Fallback if no valid parent PID was found
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error determining terminal state: {ex.Message}");
                return false; // Default to terminal if any error occurs
            }
        }

        private static void Main(string[] args)
        {
            bool newTerminal = IsNewTerminal();

            // Setup the terminal context and run the router indefinitely as a console.
            // NOTE: Driver is enabled, so you can run the terminal as a native driver program. Ensure args are passed.
            Dictionary<string, object> customProperties = new()
            {
                { "new_terminal", newTerminal }
            };
            TerminalConsoleRouterContext consoleRouterContext = new(TerminalStartMode.Console, routeOnce: !newTerminal, customProperties, args);

            // Start the host builder and run terminal router till canceled.
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(ConfigureAppConfigurationDelegate)
                .ConfigureLogging(ConfigureLoggingDelegate)
                .ConfigureServices(ConfigureServicesDelegate)
                .RunTerminalRouter<TerminalConsoleRouter, TerminalConsoleRouterContext>(consoleRouterContext);
        }
    }
}