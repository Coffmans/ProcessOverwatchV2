using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessOverwatch.Controller
{
    public class SendNotification
    {
        public string Subject { get; }
        public string Body { get; }
        public SendNotification(string subject, string body)
        {
            Subject = subject;
            Body = body;
        }
    }
}
