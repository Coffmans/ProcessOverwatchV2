using Akka.Actor;
using Akka.Configuration;
using Serilog;

namespace ProcessOverwatch.Agent
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private ActorSystem _actorSystem = null!;
        private IActorRef _processMonitorActor = null!;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                string logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ProcessOverwatch", "Logs");
                Directory.CreateDirectory(logDir); // ensures folder exists

                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.File(
                        path: Path.Combine(logDir, "log-.txt"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 7, // <- keeps only 7 most recent log files
                        outputTemplate: "[{Timestamp:yyyy-MM-dd}] [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                    )
                    .CreateLogger();

                // Initialize Akka.NET actor system
                var config = ConfigurationFactory.ParseString(@"
                    akka {
                        actor.provider = remote
                        remote.dot-netty.tcp {
                            port = 8935
                            hostname = 127.0.0.1
                        }
                    }");

                _actorSystem = ActorSystem.Create("AgentSystem", config);
                _processMonitorActor = _actorSystem.ActorOf(Props.Create(() => new ProcessMonitorActor()), "agent");

                _logger.LogInformation("Agent started at: {time}", DateTimeOffset.Now);

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Agent error");
            }
        }
    }
}
