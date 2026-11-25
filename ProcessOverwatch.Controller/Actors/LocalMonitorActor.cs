using Akka.Actor;
using ProcessOverwatch.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ProcessOverwatch.Controller.Actors
{
    public class LocalMonitorActor : ReceiveActor
    {
        private readonly IActorRef _statusUpdateActor;

        public LocalMonitorActor(IActorRef statusUpdateActor)
        {
            _statusUpdateActor = statusUpdateActor;

            ReceiveAsync<CheckProcess>(HandleCheckProcess);
        }

        public IActorRef StatusUpdateActor => _statusUpdateActor;

        private async Task HandleCheckProcess(CheckProcess msg)
        {
            // Capture before awaiting; Sender is only valid while handling the current message
            var sender = Sender;

            // Only process local processes (IPAddress is empty); check them in parallel so retries don't stack up
            await Task.WhenAll(msg.Processes
                .Where(p => string.IsNullOrEmpty(p.IPAddress))
                .Select(async process =>
                {
                    var response = await ProcessChecker.CheckAsync(process, msg.RetryCount, msg.RetryDelaySeconds, StartProcess);

                    // Send status response back to MainForm
                    sender.Tell(response);
                }));
        }

        private static void StartProcess(MonitoredProcess process)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = process.ExecutablePath,
                Arguments = process.Arguments ?? "",
                UseShellExecute = true
            });
        }
    }
}
