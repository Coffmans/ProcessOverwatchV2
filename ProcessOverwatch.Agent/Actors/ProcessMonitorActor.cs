using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using ProcessOverwatch.Shared;

namespace ProcessOverwatch.Agent
{
    public class ProcessMonitorActor : ReceiveActor
    {
        private readonly MonitoredProcess _process;
        private readonly IActorRef _notifier;

        public ProcessMonitorActor(MonitoredProcess process, IActorRef notifier)
        {
            _process = process;
            _notifier = notifier;

            Receive<CheckProcess>(_ => HandleCheckProcess());
        }

        private void HandleCheckProcess()
        {
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

        private void SendEmailNotification(bool isRunning)
        {
            var config = AppState.Config;

            try
            {
                var client = new SmtpClient(config.SmtpServer, config.SmtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(config.EmailUser, config.EmailPass)
                };

                string _emailBody = isRunning
                    ? $"The process {_process.FriendlyName} ({_process.ExecutablePath}) was restarted successfully at {DateTime.Now}."
                    : $"The process {_process.FriendlyName} ({_process.ExecutablePath}) was found not running at {DateTime.Now}.";

                var mail = new MailMessage
                {
                    From = new MailAddress(config.EmailFrom),
                    Subject = $"Process {_process.FriendlyName} not running",
                    Body = _emailBody
                };

                mail.To.Add(config.EmailTo);

                client.Send(mail);

                Log.Information($"📧 Email sent for {_process.FriendlyName}.");
            }
            catch (Exception ex)
            {
                Log.Error($"⚠️ Failed to send email: {ex.Message}");
            }
        }

        private void Logging(string message)
        {
            string logFileName = $"monitor_{DateTime.Now:yyyy-MM-dd}.log";
            File.AppendAllText(logFileName, $"[{DateTime.Now}] {message}\n");
        }
    }
}
