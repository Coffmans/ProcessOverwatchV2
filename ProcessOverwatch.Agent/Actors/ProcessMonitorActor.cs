using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ProcessOverwatch.Shared;

namespace ProcessOverwatch.Agent.Actors
{
    public class ProcessMonitorActor : ReceiveActor
    {

        public ProcessMonitorActor()
        {
            ReceiveAsync<CheckProcess>(HandleCheckProcess);
        }

        private async Task HandleCheckProcess(CheckProcess msg)
        {
            // Capture before awaiting; Sender is only valid while handling the current message
            var sender = Sender;

            // Check processes in parallel so one process's retries don't delay the others
            await Task.WhenAll(msg.Processes.Select(async process =>
            {
                var response = await ProcessChecker.CheckAsync(process, msg.RetryCount, msg.RetryDelaySeconds, StartProcess);

                // Send status response back to Controller
                sender.Tell(response);
            }));
        }

        private static void StartProcess(MonitoredProcess process)
        {
            if (Environment.UserInteractive)
            {
                // Console mode: already running on the user's desktop
                Process.Start(new ProcessStartInfo
                {
                    FileName = process.ExecutablePath,
                    Arguments = process.Arguments ?? "",
                    UseShellExecute = true
                });
            }
            else
            {
                // Service mode: launch on the logged-on user's desktop rather than in session 0 as SYSTEM
                UserSessionLauncher.StartInUserSession(process.ExecutablePath, process.Arguments ?? "");
            }
        }
    }
}
