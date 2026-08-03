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
        private static List<MonitoredProcess> processes = [];
        private static AppConfig config = new();

        private static readonly System.Text.Json.JsonSerializerOptions s_serializerOptions = new()
        {
            WriteIndented = true
        };

        public static string ProcessesFilePath  => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ProcessOverwatch", "processes.json");

        public static AppConfig GetConfig()
        {
            return config;
        }

        public static void SetConfig(AppConfig value)
        {
            config = value;
        }

        public static List<MonitoredProcess> GetProcesses()
        {
            return processes;
        }

        public static void SetProcesses(List<MonitoredProcess> value)
        {
            processes = value;
        }

        public static void LoadConfig()
        {
            if (File.Exists(ProcessesFilePath))
                SetProcesses(JsonSerializer.Deserialize<List<MonitoredProcess>>(File.ReadAllText(ProcessesFilePath)) ?? []);

            SetConfig(AppConfig.Load());
        }

        public static void SaveConfig()
        {
            File.WriteAllText(ProcessesFilePath, JsonSerializer.Serialize(GetProcesses(), s_serializerOptions));
            GetConfig().Save();
        }
    }
}
