namespace ProcessOverwatch.Shared
{
    public class Messages
    {
        public record StartProcess(string ProcessName, string ExecutablePath);
        public record StopProcess(string ProcessName);
        public record RestartProcess(string ProcessName, string ExecutablePath);
        public record GetProcessStatus(string ProcessName);
    }
}
