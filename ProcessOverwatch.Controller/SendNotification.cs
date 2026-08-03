using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessOverwatch.Controller
{
    public class SendNotification(string subject, string body)
    {
        public string Subject { get; } = subject;
        public string Body { get; } = body;
    }
}
