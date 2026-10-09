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

        public IActorRef StatusUpdateActor => _statusUpdateActor;

        private void HandleCheckProcess(CheckProcess msg)
        {
            foreach (var process in msg.Processes)
            {
                // Only process local processes (RemoteIPPort is empty or null)
                if (!string.IsNullOrEmpty(process.IPAddress))
                {
                    continue; // Skip remote processes
                }

                bool isRunning = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(process.ExecutablePath)).Length != 0;


                string status;
                bool wasRestarted = false;
                if (isRunning)
                {
                    status = $"✅ {process.FriendlyName} is running.";
                    Log.Information("✅ {FriendlyName} is running.", process.FriendlyName);
                }
                else
                {
                    status = $"❌ {process.FriendlyName} is NOT running!";
                    Log.Warning("❌ {FriendlyName} is NOT running!", process.FriendlyName);

                    if (process.RestartIfNotRunning)
                    {
                        try
                        {

                            Process.Start(new ProcessStartInfo
                            {
                                FileName = process.ExecutablePath,
                                Arguments = process.Arguments ?? "",
                                UseShellExecute = true
                            });

                            isRunning = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(process.ExecutablePath)).Length != 0;
                            if (isRunning)
                            {
                                wasRestarted = true;
                                status = $"🔁 Restarted {process.FriendlyName}.";
                                Log.Information("🔁 Restarted {FriendlyName}.", process.FriendlyName);
                            }
                            else
                            {
                                status = $"⚠️ Failed to restart {process.FriendlyName}.";
                                Log.Warning("⚠️ Failed to restart {FriendlyName}.", process.FriendlyName);
                            }
                        }
                        catch (Exception ex)
                        {
                            status = $"⚠️ Failed to restart {process.FriendlyName}";
                            Log.Error(ex, "⚠️ Failed to restart {FriendlyName}", process.FriendlyName);
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
                    status,
                    wasRestarted
                ));
            }
        }
    }
}
