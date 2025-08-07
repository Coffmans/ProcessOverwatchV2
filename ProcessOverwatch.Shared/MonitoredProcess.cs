using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ProcessOverwatch.Shared
{
    public class MonitoredProcess : INotifyPropertyChanged
    {
        public string FriendlyName { get; set; } = "";
        public string ExecutablePath { get; set; } = "";
        public bool IsEnabled { get; set; }
        public bool RestartIfNotRunning { get; set; }
        public string Arguments { get; set; } = "";
        public string IPAddress { get; set; } = "";

        public string Status { get; set; } = "";

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
