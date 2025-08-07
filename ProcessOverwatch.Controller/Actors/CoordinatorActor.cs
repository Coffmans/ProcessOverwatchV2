using Akka.Actor;
using ProcessOverwatch.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessOverwatch.Controller.Actors
{
    public class CoordinatorActor : ReceiveActor
    {
        private readonly IActorRef _statusUpdateActor;
        private readonly IActorRef _localMonitorActor;
        public CoordinatorActor(IActorRef statusUpdateActor, IActorRef localMonitorActor)
        {
            _statusUpdateActor = statusUpdateActor;
            _localMonitorActor = localMonitorActor;

            Receive<CheckProcess>(msg =>
            {
                _localMonitorActor.Tell(msg, Self);
            });

            Receive<ProcessStatusResponse>(msg =>
            {
                _statusUpdateActor.Tell(msg);
            });
        }
    }
}
