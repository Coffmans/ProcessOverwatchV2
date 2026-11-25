using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessOverwatch.Shared
{
    /// <param name="RetryCount">Additional verify/restart attempts after the first failure before the process is reported down.</param>
    /// <param name="RetryDelaySeconds">Wait between attempts, and before verifying that a started process stayed up.</param>
    public record CheckProcess(List<MonitoredProcess> Processes, int RetryCount = 0, int RetryDelaySeconds = 10);
    public record ProcessStatusResponse(string FriendlyName, string ExecutablePath, bool IsRunning, string MachineName, string RemoteIPPort, string Status, bool WasRestarted = false);
}
