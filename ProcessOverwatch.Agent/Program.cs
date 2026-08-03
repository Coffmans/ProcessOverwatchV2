
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProcessOverwatch.Agent;
using Serilog;
using System;
using System.Diagnostics;
using System.ServiceProcess;

namespace ProcessOverwatch.Agent
{
    static class Program
    {
        static async Task Main()
        {
            try
            {
                ConfigureLogging();

                // Set up dependency injection
                var services = new ServiceCollection();
                services.AddLogging(builder =>
                {
                    builder.AddEventLog(); // Log to Event Viewer
                    if (Environment.UserInteractive)
                    {
                        builder.AddConsole(); // Log to console when running interactively
                    }
                });
                services.AddSingleton<ProcessOverwatchService>();

                var serviceProvider = services.BuildServiceProvider();
                var service = serviceProvider.GetService<ProcessOverwatchService>()!;

                if (Environment.UserInteractive)
                {
                    // Run as console app for debugging
                    EventLog.WriteEntry("ProcessOverwatchAgent", "Running in console mode.", EventLogEntryType.Information);
                    Log.Information("ProcessOverwatchAgent starting in console mode at {Time}", DateTimeOffset.Now);
                    await service.StartAsync();
                    Console.WriteLine("Service running in console mode. Press any key to stop...");
                    Console.ReadKey();
                    await service.StopAsync();
                }
                else
                {
                    // Run as Windows service
                    EventLog.WriteEntry("ProcessOverwatchAgent", "Running as Windows service.", EventLogEntryType.Information);
                    Log.Information("ProcessOverwatchAgent starting at {Time}", DateTimeOffset.Now);
                    ServiceBase.Run(service);
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("ProcessOverwatchService", $"Error in Program.cs: {ex}", EventLogEntryType.Error);
            }
        }

        private static void ConfigureLogging()
        {
            string logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ProcessOverwatch", "Logs");
            Directory.CreateDirectory(logDir); // ensures folder exists

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(
                    path: Path.Combine(logDir, "log-.txt"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7, // <- keeps only 7 most recent log files
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();
        }
    }
}