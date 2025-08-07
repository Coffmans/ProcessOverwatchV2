using Akka.Actor;
using ProcessOverwatch.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessOverwatch.Controller.Actors
{
    public class StatusUpdateActor : ReceiveActor
    {
        private readonly MainForm _form;

        public StatusUpdateActor(MainForm form)
        {
            _form = form;
            Receive<ProcessStatusResponse>(HandleStatusResponse);
        }

        private void HandleStatusResponse(ProcessStatusResponse response)
        {
            if (_form.InvokeRequired)
            {
                _form.Invoke(new Action<ProcessStatusResponse>(_form.UpdateProcessStatus), response);
            }
            else
            {
                _form.UpdateProcessStatus(response);
            }

            // Notify EmailNotifierActor for non-running processes
            if (!response.IsRunning)
            {
                _form._notifierActor.Tell(response);
            }
        }
    }
}
