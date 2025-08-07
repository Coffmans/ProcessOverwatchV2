using Microsoft.Extensions.Logging;
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
        private CancellationTokenSource _cts;

        public ProcessOverwatchService(ILogger<ProcessOverwatchService> logger)
        {
            _logger = logger;
            ServiceName = "ProcessOverwatchService";
            _cts = new CancellationTokenSource();
        }

        // Protected method for Windows service start
        protected override void OnStart(string[] args)
        {
            try
            {
                EventLog.WriteEntry(ServiceName, "OnStart called.", EventLogEntryType.Information);
                _logger.LogInformation("Service starting at {time}", DateTimeOffset.Now);

                // Test file system access to diagnose logging issue
                string logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ProcessOverwatch", "Logs");
                Directory.CreateDirectory(logDir);
                string logFile = Path.Combine(logDir, "test.txt");
                File.WriteAllText(logFile, $"Service started at {DateTimeOffset.Now}");
                EventLog.WriteEntry(ServiceName, $"Wrote to log file: {logFile}", EventLogEntryType.Information);

                // Start the main task
                _mainTask = Task.Run(() => ExecuteAsync(_cts.Token), _cts.Token);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ServiceName, $"Error in OnStart: {ex}", EventLogEntryType.Error);
                _logger.LogError(ex, "Error in OnStart");
                throw; // Rethrow to notify SCM of failure
            }
        }

        // Protected method for Windows service stop
        protected override void OnStop()
        {
            try
            {
                EventLog.WriteEntry(ServiceName, "OnStop called.", EventLogEntryType.Information);
                _logger.LogInformation("Service stopping at {time}", DateTimeOffset.Now);

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
        public async Task StartAsync(string[] args)
        {
            try
            {
                EventLog.WriteEntry(ServiceName, "StartAsync called (console mode).", EventLogEntryType.Information);
                _logger.LogInformation("StartAsync (console mode) starting at {time}", DateTimeOffset.Now);

                // Same logic as OnStart
                string logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ProcessOverwatch", "Logs");
                Directory.CreateDirectory(logDir);
                string logFile = Path.Combine(logDir, "test.txt");
                File.WriteAllText(logFile, $"Console mode started at {DateTimeOffset.Now}");
                EventLog.WriteEntry(ServiceName, $"Wrote to log file (console mode): {logFile}", EventLogEntryType.Information);

                _mainTask = Task.Run(() => ExecuteAsync(_cts.Token), _cts.Token);
                await Task.CompletedTask; // For async signature
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ServiceName, $"Error in StartAsync: {ex}", EventLogEntryType.Error);
                _logger.LogError(ex, "Error in StartAsync");
                throw;
            }
        }

        // Public method for console mode stop
        public async Task StopAsync()
        {
            try
            {
                EventLog.WriteEntry(ServiceName, "StopAsync called (console mode).", EventLogEntryType.Information);
                _logger.LogInformation("StopAsync (console mode) stopping at {time}", DateTimeOffset.Now);

                // Same logic as OnStop
                _cts.Cancel();
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
                _logger.LogInformation("ExecuteAsync started at {time}", DateTimeOffset.Now);

                // Minimal logic to keep the service running
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                EventLog.WriteEntry(ServiceName, $"Error in ExecuteAsync: {ex}", EventLogEntryType.Error);
                _logger.LogError(ex, "Error in ExecuteAsync");
            }
        }
    }
}