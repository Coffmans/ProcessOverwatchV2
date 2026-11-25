using Serilog;
using System.Diagnostics;

namespace ProcessOverwatch.Shared
{
    /// <summary>
    /// Checks a monitored process and, when it is down, re-verifies or restarts it up to the configured
    /// number of retries before reporting it as down. Shared by the controller (local processes) and the agent.
    /// </summary>
    public static class ProcessChecker
    {
        public static async Task<ProcessStatusResponse> CheckAsync(
            MonitoredProcess process, int retryCount, int retryDelaySeconds, Action<MonitoredProcess> startProcess)
        {
            string machine = Environment.MachineName;
            var retryDelay = TimeSpan.FromSeconds(Math.Max(1, retryDelaySeconds));
            retryCount = Math.Max(0, retryCount);

            ProcessStatusResponse Response(bool isRunning, string status, bool wasRestarted = false) =>
                new(process.FriendlyName, process.ExecutablePath, isRunning, machine, process.IPAddress, status, wasRestarted);

            if (IsRunning(process))
            {
                Log.Information("✅ {FriendlyName} is running on {Machine}.", process.FriendlyName, machine);
                return Response(true, $"✅ {process.FriendlyName} is running on {machine}.");
            }

            Log.Warning("❌ {FriendlyName} is NOT running on {Machine}.", process.FriendlyName, machine);

            if (!process.RestartIfNotRunning)
            {
                // Re-verify before reporting, in case the process was only briefly down (e.g. restarting itself)
                for (int retry = 1; retry <= retryCount; retry++)
                {
                    await Task.Delay(retryDelay);
                    if (IsRunning(process))
                    {
                        Log.Information("✅ {FriendlyName} came back on its own (re-check {Retry} of {RetryCount}).", process.FriendlyName, retry, retryCount);
                        return Response(true, $"✅ {process.FriendlyName} is running on {machine}.");
                    }
                    Log.Warning("❌ {FriendlyName} still NOT running (re-check {Retry} of {RetryCount}).", process.FriendlyName, retry, retryCount);
                }

                string checkedText = retryCount > 0 ? $" (checked {retryCount + 1} times)" : "";
                return Response(false, $"❌ {process.FriendlyName} is NOT running on {machine}{checkedText}. Restart is not enabled.");
            }

            int attempts = retryCount + 1;
            string? lastError = null;
            for (int attempt = 1; attempt <= attempts; attempt++)
            {
                try
                {
                    startProcess(process);
                    lastError = null;
                }
                catch (Exception ex)
                {
                    lastError = ex.Message;
                    Log.Error(ex, "⚠️ Start attempt {Attempt} of {Attempts} failed for {FriendlyName}.", attempt, attempts, process.FriendlyName);
                }

                // Give the process time to either settle or crash before confirming it stayed up
                await Task.Delay(retryDelay);

                if (IsRunning(process))
                {
                    string attemptText = attempt > 1 ? $" after {attempt} attempts" : "";
                    Log.Information("🔁 Restarted {FriendlyName} on {Machine}{AttemptText}.", process.FriendlyName, machine, attemptText);
                    return Response(true, $"🔁 Restarted {process.FriendlyName} on {machine}{attemptText}.", wasRestarted: true);
                }

                Log.Warning("⚠️ {FriendlyName} not running after start attempt {Attempt} of {Attempts}.", process.FriendlyName, attempt, attempts);
            }

            string reason = lastError is null ? "" : $" Last error: {lastError}";
            return Response(false, $"⚠️ {process.FriendlyName} is NOT running on {machine} and could not be started after {attempts} attempt(s).{reason}");
        }

        public static bool IsRunning(MonitoredProcess process) =>
            Process.GetProcessesByName(Path.GetFileNameWithoutExtension(process.ExecutablePath)).Length != 0;
    }
}
