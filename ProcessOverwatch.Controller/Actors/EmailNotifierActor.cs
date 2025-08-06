using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace ProcessOverwatch.Controller.Actors
{
    public class EmailNotifierActor : ReceiveActor
    {
        private readonly AppConfig _config;
        public EmailNotifierActor(AppConfig config)
        {
            _config = config;
            Receive<SendNotification>(HandleSendNotification);
        }

        private void HandleSendNotification(SendNotification msg)
        {
            try
            {
                if( String.IsNullOrEmpty(_config.SmtpServer) )
                {
                    Log.Warning("SMTP server is not configured. Skipping email notification.");
                    return;
                }
                var client = new SmtpClient(_config.SmtpServer, _config.SmtpPort)
                {
                    Credentials = new NetworkCredential(_config.SmtpUser, _config.SmtpPassword),
                    EnableSsl = true
                };

                var mail = new MailMessage(_config.EmailFrom, _config.EmailTo, msg.Subject, msg.Body);
                client.Send(mail);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Email notification failed"); 
            }
        }
    }
}
