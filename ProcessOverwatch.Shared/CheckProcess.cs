using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessOverwatch.Shared
{
    public record CheckProcess(List<MonitoredProcess> Processes);
    public record ProcessStatusResponse(string FriendlyName, string ExecutablePath, bool IsRunning, string MachineName, string RemoteIPPort, string Status);
}
