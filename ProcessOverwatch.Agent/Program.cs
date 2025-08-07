
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.ServiceProcess;
using ProcessOverwatch.Agent;
using System.Diagnostics;

using System;

namespace ProcessOverwatch.Agent
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                EventLog.WriteEntry("ProcessOverwatchService", "Program.cs started.", EventLogEntryType.Information);

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
                    EventLog.WriteEntry("ProcessOverwatchService", "Running in console mode.", EventLogEntryType.Information);
                    await service.StartAsync(args);
                    Console.WriteLine("Service running in console mode. Press any key to stop...");
                    Console.ReadKey();
                    await service.StopAsync();
                }
                else
                {
                    // Run as Windows service
                    EventLog.WriteEntry("ProcessOverwatchService", "Running as Windows service.", EventLogEntryType.Information);
                    ServiceBase.Run(service);
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("ProcessOverwatchService", $"Error in Program.cs: {ex}", EventLogEntryType.Error);
            }
        }
    }
}