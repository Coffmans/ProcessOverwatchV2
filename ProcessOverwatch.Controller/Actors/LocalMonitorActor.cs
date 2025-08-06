using Akka.Actor;
using ProcessOverwatch.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace ProcessOverwatch.Controller.Actors
{
    public class LocalMonitorActor : ReceiveActor
    {
        public LocalMonitorActor()
        {
            Receive<CheckProcess>(HandleCheckProcess);
        }
        
        private void HandleCheckProcess(CheckProcess msg)
        {
            List<MonitoredProcess> _processes = msg._processes;

            foreach(var process in _processes)
            {
            }
            bool isRunning = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(_process.ExecutablePath))
                                     .Any();

            if (isRunning)
            {
                Log.Information($"✅ {_process.FriendlyName} is running.");
                return;
            }

            Log.Warning($"❌ {_process.FriendlyName} is NOT running!");

            if (_process.RestartIfNotRunning)
            {
                try
                {
                    Process.Start(_process.ExecutablePath);
                    Log.Information($"🔁 Restarted {_process.FriendlyName}.");
                    isRunning = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(_process.ExecutablePath))
                         .Any();
                }
                catch (Exception ex)
                {
                    Log.Error($"⚠️ Failed to restart {_process.FriendlyName}: {ex.Message}");
                }
            }

            SendEmailNotification(isRunning);
        }

    }
}
