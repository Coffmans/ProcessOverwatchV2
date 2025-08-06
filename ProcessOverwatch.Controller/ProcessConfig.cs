using ProcessOverwatch.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProcessOverwatch.Controller
{
    public static class ProcessConfig
    {
        public static List<MonitoredProcess> Processes = new();
        public static AppConfig Config = new();
        public static string _processesFilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ProcessOverwatch", "processes.json");

        public static void LoadConfig()
        {
            if (File.Exists(_processesFilePath))
                Processes = JsonSerializer.Deserialize<List<MonitoredProcess>>(File.ReadAllText(_processesFilePath)) ?? new();

            Config = AppConfig.Load();
        }

        public static void SaveConfig()
        {
            File.WriteAllText(_processesFilePath, JsonSerializer.Serialize(Processes, new JsonSerializerOptions { WriteIndented = true }));
            Config.Save();
        }
    }
}
