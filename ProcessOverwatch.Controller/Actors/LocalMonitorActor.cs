using Akka.Actor;
using ProcessOverwatch.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace ProcessOverwatch.Controller.Actors
{
    public class LocalMonitorActor : ReceiveActor
    {
        private readonly IActorRef _statusUpdateActor;

        public LocalMonitorActor(IActorRef statusUpdateActor)
        {
            _statusUpdateActor = statusUpdateActor;

            Receive<CheckProcess>(HandleCheckProcess);
        }

        private void HandleCheckProcess(CheckProcess msg)
        {
            foreach (var process in msg.Processes)
            {
                // Only process local processes (RemoteIPPort is empty or null)
                if (!string.IsNullOrEmpty(process.IPAddress))
                {
                    continue; // Skip remote processes
                }

                string status = "Unknown";
                bool isRunning = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(process.ExecutablePath)).Any();

                if (isRunning)
                {
                    status = $"✅ {process.FriendlyName} is running.";
                    Log.Information(status);
                }
                else
                {
                    status = $"❌ {process.FriendlyName} is NOT running!";
                   Log.Warning(status);

                    if (process.RestartIfNotRunning)
                    {
                        try
                        {
                            
                            Process.Start(process.ExecutablePath);
                            status = $"🔁 Restarted {process.FriendlyName}.";
                            Log.Information(status);
                            
                            isRunning = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(process.ExecutablePath)).Any();
                        }
                        catch (Exception ex)
                        {
                            status = $"⚠️ Failed to restart {process.FriendlyName}";
                            Log.Error($"⚠️ Failed to restart {process.FriendlyName}: {ex.Message}");
                        }
                    }
                }

                // Send status response back to MainForm
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
}
