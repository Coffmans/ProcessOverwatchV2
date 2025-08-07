using Akka.Actor;
using ProcessOverwatch.Shared;
using System;
using System.Net.Mail;
using System.Net;
using Serilog;

namespace ProcessOverwatch.Controller.Actors
{
    public class EmailNotifierActor : ReceiveActor
    {
        private readonly AppConfig _config;

        public EmailNotifierActor(AppConfig config)
        {
            _config = config;
            Receive<ProcessStatusResponse>(HandleStatusResponse);
        }

        private void HandleStatusResponse(ProcessStatusResponse response)
        {
            // Only send emails for non-running processes
            if (response.IsRunning && !response.Status.Contains("is NOT running") && !response.Status.Contains("Restart"))
            {
                return;
            }

            try
            {
                var client = new SmtpClient(_config.SmtpServer, _config.SmtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(_config.EmailUser, _config.EmailPass)
                };

                string emailBody = response.Status;

                //string emailBody = $"The process {response.FriendlyName} ({response.ExecutablePath}) was found not running on {response.MachineName} at {DateTime.Now:yyyy-MM-dd HH:mm:ss}.";

                var mail = new MailMessage
                {
                    From = new MailAddress(_config.EmailFrom),
                    Subject = $"Process {response.FriendlyName} Not Running",
                    Body = emailBody
                };
                mail.To.Add(_config.EmailTo);
                client.Send(mail);
                Log.Information($"📧 Email sent for {response.FriendlyName} on {response.MachineName}.");
            }
            catch (Exception ex)
            {
                Log.Error($"⚠️ Failed to send email for {response.FriendlyName}: {ex.Message}");
            }
        }
    }
}