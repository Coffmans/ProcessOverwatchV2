using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessOverwatch.Shared
{
    public class MonitoredProcess
    {
        public string FriendlyName { get; set; } = "";
        public string ExecutablePath { get; set; } = "";
        public bool IsEnabled { get; set; }
        public bool RestartIfNotRunning { get; set; }
        public string Arguments { get; set; } = "";
        public string RemoteIPAndPort { get; set; } = "";
    }
}
