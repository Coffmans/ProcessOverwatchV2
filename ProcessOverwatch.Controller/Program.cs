using Serilog;
using System.IO;
using System.Threading;

namespace ProcessOverwatch.Controller
{
    internal static class Program
    {
        public static Mutex Mutex { get; set; } = null!;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            const string mutexName = "ProcessOverwatch";
            Mutex = new Mutex(true, mutexName, out bool createdNew);
            if (!createdNew)
            {
                // Another instance is already running
                MessageBox.Show("Another instance of Process Overwatch is already running.", "Instance Already Running", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            ConfigureLogging();
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());

            Mutex.ReleaseMutex();
        }

        private static void ConfigureLogging()
        {
            string logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ProcessOverwatch", "Logs");
            Directory.CreateDirectory(logDir); // ensures folder exists

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(
                    path: Path.Combine(logDir, "log-.txt"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7, // <- keeps only 7 most recent log files
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();

            Log.Information("Logger initialized. Logs at: {Path}", logDir);
        }
    }
}