using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessOverwatch.Controller
{
    public class AppConfig
    {
        public string SmtpServer { get; set; } = "";
        public int SmtpPort { get; set; } = 587;
        public string SmtpUser { get; set; } = "";
        public string SmtpPassword { get; set; } = "";

        public string EmailFrom { get; set; } = "";
        public string EmailTo { get; set; } = "";
        public string EmailUser { get; set; } = "";
        public string EmailPass { get; set; } = "";
        public int MonitorIntervalMinutes { get; set; } = 5;
        public bool AutoStartMonitoring { get; set; } = false;

        //public static string ConfigFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        public static string ConfigFilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ProcessOverwatch", "config.json");

        public static AppConfig Load()
        {
            if (!File.Exists(ConfigFilePath))
                return new AppConfig();

            var json = File.ReadAllText(ConfigFilePath);
            return System.Text.Json.JsonSerializer.Deserialize<AppConfig>(json)
                   ?? new AppConfig();
        }

        public void Save()
        {
            var json = System.Text.Json.JsonSerializer.Serialize(this, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(ConfigFilePath, json);
        }
    }
}
