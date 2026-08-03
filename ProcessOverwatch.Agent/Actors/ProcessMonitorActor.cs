using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using ProcessOverwatch.Shared;

namespace ProcessOverwatch.Agent.Actors
{
    public class ProcessMonitorActor : ReceiveActor
    {

        public ProcessMonitorActor()
        {
            Receive<CheckProcess>(HandleCheckProcess);
        }

        private void HandleCheckProcess(CheckProcess msg)
        {
            foreach (var process in msg.Processes)
            {
                CheckProcessStatus(process);
            }
        }
        private void CheckProcessStatus(MonitoredProcess process)
        {
            string status;
            bool isRunning = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(process.ExecutablePath)).Length != 0;
            if (isRunning)
            {
                status = $"✅ {process.FriendlyName} is running on {Environment.MachineName}.";
                Log.Information(status);
            }
            else
            {
                status = $"❌ {process.FriendlyName} is NOT running on {Environment.MachineName}!";
                Log.Information(status);
                if (process.RestartIfNotRunning)
                {
                    try
                    {
                        var startInfo = new ProcessStartInfo
                        {
                            FileName = process.ExecutablePath,
                            Arguments = process.Arguments ?? "",
                            UseShellExecute = true
                        };

                        Process.Start(startInfo);
                        isRunning = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(process.ExecutablePath)).Length != 0;

                        if (isRunning)
                        {
                            status = $"🔁 Restarted {process.FriendlyName} on {Environment.MachineName}!";
                        }
                        else
                            status = $"⚠️ Failed to restart {process.FriendlyName} on {Environment.MachineName}.";
                        Log.Information(status);

                    }
                    catch (Exception ex)
                    {
                        status = $"⚠️ Failed to restart {process.FriendlyName} on {Environment.MachineName} - Check Logging!";
                        Log.Error(ex, "⚠️ Failed to restart {FriendlyName}", process.FriendlyName);
                    }
                }
            }

            // Send status response back to Controller
            Sender.Tell(new ProcessStatusResponse(
                process.FriendlyName,
                process.ExecutablePath,
                isRunning,
                Environment.MachineName,
                process.IPAddress,
                status
            ));
        }
    }
}
