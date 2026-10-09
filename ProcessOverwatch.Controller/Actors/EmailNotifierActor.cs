using Akka.Actor;
using ProcessOverwatch.Shared;
using System;
using System.Net.Mail;
using System.Net;
using Serilog;

namespace ProcessOverwatch.Controller.Actors
{
    /// <summary>Sent when a remote machine or its agent could not be contacted during a check.</summary>
    public record RemoteMachineUnreachable(string IPAddress, bool MachineResponds, IReadOnlyList<string> ProcessNames);

    /// <summary>Sent when a remote agent was contacted successfully, clearing any earlier unreachable alert.</summary>
    public record RemoteMachineReachable(string IPAddress);

    public class EmailNotifierActor : ReceiveActor
    {
        // Problems already emailed about; an alert is only re-sent after the problem clears and comes back
        private readonly HashSet<string> _activeAlerts = [];

        public EmailNotifierActor()
        {
            Receive<ProcessStatusResponse>(HandleStatusResponse);
            Receive<RemoteMachineUnreachable>(HandleUnreachable);
            Receive<RemoteMachineReachable>(msg => _activeAlerts.Remove(MachineKey(msg.IPAddress)));
        }

        private void HandleStatusResponse(ProcessStatusResponse response)
        {
            string key = $"process|{response.RemoteIPPort}|{response.ExecutablePath}";
            string location = string.IsNullOrEmpty(response.RemoteIPPort)
                ? response.MachineName
                : $"{response.MachineName} ({response.RemoteIPPort})";

            if (response.WasRestarted)
            {
                _activeAlerts.Remove(key);
                SendEmail(
                    $"{response.FriendlyName} was down and has been restarted on {location}",
                    $"{response.FriendlyName} was found NOT running on {location} at {DateTime.Now:yyyy-MM-dd HH:mm:ss} and was restarted.\n\n" +
                    $"Executable: {response.ExecutablePath}\n" +
                    $"Status: {response.Status}");
            }
            else if (!response.IsRunning)
            {
                if (!_activeAlerts.Add(key))
                    return;

                SendEmail(
                    $"{response.FriendlyName} is NOT running on {location}",
                    $"{response.FriendlyName} was found NOT running on {location} at {DateTime.Now:yyyy-MM-dd HH:mm:ss}.\n\n" +
                    $"Executable: {response.ExecutablePath}\n" +
                    $"Status: {response.Status}\n\n" +
                    "You will not be notified again until the process has been seen running.");
            }
            else
            {
                _activeAlerts.Remove(key);
            }
        }

        private void HandleUnreachable(RemoteMachineUnreachable msg)
        {
            if (!_activeAlerts.Add(MachineKey(msg.IPAddress)))
                return;

            string problem = msg.MachineResponds
                ? $"The machine at {msg.IPAddress} responds to ping, but the ProcessOverwatch Agent is not responding."
                : $"The machine at {msg.IPAddress} is not reachable on the network.";

            SendEmail(
                $"Cannot reach {msg.IPAddress}",
                $"{problem}\n\nChecked at {DateTime.Now:yyyy-MM-dd HH:mm:ss}. The following processes could not be checked:\n" +
                string.Join("\n", msg.ProcessNames.Select(n => $"  - {n}")) +
                "\n\nYou will not be notified again until the agent is reachable.");
        }

        private static string MachineKey(string ipAddress) => $"machine|{ipAddress}";

        private static void SendEmail(string subject, string body)
        {
            // Read config on each send so changes made in the settings form apply without a restart
            var config = AppState.GetConfig();
            if (string.IsNullOrWhiteSpace(config.SmtpServer) || string.IsNullOrWhiteSpace(config.EmailFrom) || string.IsNullOrWhiteSpace(config.EmailTo))
            {
                Log.Warning("📧 Email settings are incomplete; not sending: {Subject}", subject);
                return;
            }

            try
            {
                using var client = new SmtpClient(config.SmtpServer, config.SmtpPort) { EnableSsl = config.SmtpEnableSsl };
                if (!string.IsNullOrWhiteSpace(config.SmtpUser))
                    client.Credentials = new NetworkCredential(config.SmtpUser, config.SmtpPassword);

                using var mail = new MailMessage
                {
                    From = new MailAddress(config.EmailFrom),
                    Subject = $"[ProcessOverwatch] {subject}",
                    Body = body
                };
                foreach (var recipient in config.EmailTo.Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    mail.To.Add(recipient);

                client.Send(mail);
                Log.Information("📧 Email sent: {Subject}", subject);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "⚠️ Failed to send email: {Subject}", subject);
            }
        }
    }
}
