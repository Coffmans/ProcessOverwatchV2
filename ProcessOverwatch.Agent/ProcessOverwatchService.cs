using Akka.Actor;
using Akka.Configuration;
using Microsoft.Extensions.Logging;
using ProcessOverwatch.Agent.Actors;
using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessOverwatch.Agent
{
    public class ProcessOverwatchService : ServiceBase
    {
        private readonly ILogger<ProcessOverwatchService> _logger;
        private Task _mainTask = null!;
        private readonly CancellationTokenSource _cts;
        private ActorSystem? _actorSystem;

        public ProcessOverwatchService(ILogger<ProcessOverwatchService> logger)
        {
            _logger = logger;
            ServiceName = "ProcessOverwatchAgent";
            _cts = new CancellationTokenSource();
        }

        // Protected method for Windows service start
        protected override void OnStart(string[] args)
        {
            try
            {
                EventLog.WriteEntry(ServiceName, "OnStart called.", EventLogEntryType.Information);

                // Start the main task
                _mainTask = Task.Run(() => ExecuteAsync(_cts.Token), _cts.Token);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ServiceName, $"Error in OnStart: {ex}", EventLogEntryType.Error);
                _logger.LogError(ex, "Error in OnStart");
            }
        }

        // Protected method for Windows service stop
        protected override void OnStop()
        {
            try
            {
                EventLog.WriteEntry(ServiceName, "OnStop called.", EventLogEntryType.Information);

                // Signal cancellation and wait for the main task to complete
                _cts.Cancel();
                _mainTask?.Wait(5000); // Wait up to 5 seconds for graceful shutdown
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ServiceName, $"Error in OnStop: {ex}", EventLogEntryType.Error);
                _logger.LogError(ex, "Error in OnStop");
            }
            finally
            {
                _cts.Dispose();
            }
        }

        // Public method for console mode start
        public async Task StartAsync()
        {
            try
            {
                EventLog.WriteEntry(ServiceName, "StartAsync called (console mode).", EventLogEntryType.Information);

                // Same logic as OnStart
                string logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ProcessOverwatch", "Logs");
                Directory.CreateDirectory(logDir);

                _mainTask = Task.Run(() => ExecuteAsync(_cts.Token), _cts.Token);
                await Task.CompletedTask; // For async signature
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ServiceName, $"Error in StartAsync: {ex}", EventLogEntryType.Error);
                _logger.LogError(ex, "Error in StartAsync");
            }
        }

        // Public method for console mode stop
        public async Task StopAsync()
        {
            try
            {
                EventLog.WriteEntry(ServiceName, "StopAsync called (console mode).", EventLogEntryType.Information);

                // Same logic as OnStop
                await _cts.CancelAsync();
                if (_mainTask != null)
                {
                    await Task.WhenAny(_mainTask, Task.Delay(5000)); // Wait up to 5 seconds
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ServiceName, $"Error in StopAsync: {ex}", EventLogEntryType.Error);
                _logger.LogError(ex, "Error in StopAsync");
            }
            finally
            {
                _cts.Dispose();
            }
        }

        private async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                EventLog.WriteEntry(ServiceName, "ExecuteAsync started.", EventLogEntryType.Information);

                var config = ConfigurationFactory.ParseString(@"
                    akka {
                        actor {
                            provider = remote
                        }
                        remote {
                            dot-netty.tcp {
                                hostname = ""0.0.0.0""
                                public-hostname = ""192.168.1.139""
                                port = 8935
                            }
                        }
                    }");

                _actorSystem = ActorSystem.Create("ProcessOverwatchAgent", config);
                _actorSystem.ActorOf(Props.Create(() => new ProcessMonitorActor()), "agent");

                EventLog.WriteEntry(ServiceName, "Actor system started, listening on port 8935.", EventLogEntryType.Information);

                // Keep running until cancelled
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                EventLog.WriteEntry(ServiceName, $"Error in ExecuteAsync: {ex}", EventLogEntryType.Error);
                _logger.LogError(ex, "Error in ExecuteAsync");
            }
            finally
            {
                if (_actorSystem != null)
                    await _actorSystem.Terminate();
            }
        }
    }
}