using System.ComponentModel;
using System.Runtime.InteropServices;

namespace ProcessOverwatch.Agent
{
    /// <summary>
    /// Starts a process on the interactive desktop of the logged-on user. A service runs in session 0,
    /// which has no visible desktop, so Process.Start would launch the app invisibly as the service account.
    /// Requires the service to run as LocalSystem (needed for WTSQueryUserToken).
    /// </summary>
    internal static class UserSessionLauncher
    {
        /// <summary>
        /// Launches the executable as the active desktop user and returns the new process id.
        /// Throws InvalidOperationException if no user is logged on.
        /// </summary>
        public static int StartInUserSession(string executablePath, string arguments)
        {
            uint sessionId = GetActiveUserSessionId()
                ?? throw new InvalidOperationException("No user is logged on to the machine; a desktop application cannot be started.");

            if (!WTSQueryUserToken(sessionId, out var impersonationToken))
                throw new Win32Exception(Marshal.GetLastWin32Error(), $"WTSQueryUserToken failed for session {sessionId}.");

            IntPtr primaryToken = IntPtr.Zero;
            IntPtr environment = IntPtr.Zero;
            try
            {
                if (!DuplicateTokenEx(impersonationToken, TOKEN_ALL_ACCESS, IntPtr.Zero,
                        SecurityImpersonation, TokenPrimary, out primaryToken))
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "DuplicateTokenEx failed.");

                // Gives the app the user's environment (PATH, %APPDATA%, etc.) instead of SYSTEM's
                if (!CreateEnvironmentBlock(out environment, primaryToken, false))
                    environment = IntPtr.Zero;

                var startupInfo = new STARTUPINFO
                {
                    cb = Marshal.SizeOf<STARTUPINFO>(),
                    lpDesktop = @"winsta0\default",
                    dwFlags = STARTF_USESHOWWINDOW,
                    wShowWindow = SW_SHOWNORMAL
                };

                string commandLine = string.IsNullOrWhiteSpace(arguments)
                    ? $"\"{executablePath}\""
                    : $"\"{executablePath}\" {arguments}";

                // Many desktop apps expect to start in their own folder
                string? workingDirectory = Path.GetDirectoryName(executablePath);

                if (!CreateProcessAsUser(primaryToken, null, commandLine, IntPtr.Zero, IntPtr.Zero, false,
                        CREATE_UNICODE_ENVIRONMENT | CREATE_NEW_CONSOLE, environment, workingDirectory,
                        ref startupInfo, out var processInfo))
                    throw new Win32Exception(Marshal.GetLastWin32Error(), $"CreateProcessAsUser failed for '{executablePath}'.");

                CloseHandle(processInfo.hThread);
                CloseHandle(processInfo.hProcess);
                return processInfo.dwProcessId;
            }
            finally
            {
                if (environment != IntPtr.Zero) DestroyEnvironmentBlock(environment);
                if (primaryToken != IntPtr.Zero) CloseHandle(primaryToken);
                CloseHandle(impersonationToken);
            }
        }

        /// <summary>
        /// Prefers the physical console session; falls back to any active (e.g. RDP) session.
        /// </summary>
        private static uint? GetActiveUserSessionId()
        {
            uint consoleSession = WTSGetActiveConsoleSessionId();
            if (consoleSession != INVALID_SESSION_ID && WTSQueryUserToken(consoleSession, out var token))
            {
                CloseHandle(token);
                return consoleSession;
            }

            if (!WTSEnumerateSessions(IntPtr.Zero, 0, 1, out var sessionInfo, out int count))
                return null;

            try
            {
                int size = Marshal.SizeOf<WTS_SESSION_INFO>();
                for (int i = 0; i < count; i++)
                {
                    var info = Marshal.PtrToStructure<WTS_SESSION_INFO>(sessionInfo + i * size);
                    if (info.State == WTS_CONNECTSTATE_CLASS.WTSActive)
                        return info.SessionID;
                }
            }
            finally
            {
                WTSFreeMemory(sessionInfo);
            }

            return null;
        }

        #region Native

        private const uint INVALID_SESSION_ID = 0xFFFFFFFF;
        private const uint TOKEN_ALL_ACCESS = 0xF01FF;
        private const int SecurityImpersonation = 2;
        private const int TokenPrimary = 1;
        private const uint CREATE_UNICODE_ENVIRONMENT = 0x00000400;
        private const uint CREATE_NEW_CONSOLE = 0x00000010;
        private const int STARTF_USESHOWWINDOW = 0x00000001;
        private const short SW_SHOWNORMAL = 1;

        private enum WTS_CONNECTSTATE_CLASS
        {
            WTSActive, WTSConnected, WTSConnectQuery, WTSShadow, WTSDisconnected,
            WTSIdle, WTSListen, WTSReset, WTSDown, WTSInit
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WTS_SESSION_INFO
        {
            public uint SessionID;
            [MarshalAs(UnmanagedType.LPStr)] public string pWinStationName;
            public WTS_CONNECTSTATE_CLASS State;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct STARTUPINFO
        {
            public int cb;
            public string? lpReserved;
            public string? lpDesktop;
            public string? lpTitle;
            public int dwX, dwY, dwXSize, dwYSize, dwXCountChars, dwYCountChars, dwFillAttribute;
            public int dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2, hStdInput, hStdOutput, hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [DllImport("kernel32.dll")]
        private static extern uint WTSGetActiveConsoleSessionId();

        [DllImport("wtsapi32.dll", SetLastError = true)]
        private static extern bool WTSQueryUserToken(uint sessionId, out IntPtr token);

        [DllImport("wtsapi32.dll", SetLastError = true)]
        private static extern bool WTSEnumerateSessions(IntPtr hServer, int reserved, int version,
            out IntPtr sessionInfo, out int count);

        [DllImport("wtsapi32.dll")]
        private static extern void WTSFreeMemory(IntPtr memory);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool DuplicateTokenEx(IntPtr existingToken, uint desiredAccess, IntPtr tokenAttributes,
            int impersonationLevel, int tokenType, out IntPtr newToken);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CreateProcessAsUser(IntPtr token, string? applicationName, string commandLine,
            IntPtr processAttributes, IntPtr threadAttributes, bool inheritHandles, uint creationFlags,
            IntPtr environment, string? currentDirectory, ref STARTUPINFO startupInfo, out PROCESS_INFORMATION processInformation);

        [DllImport("userenv.dll", SetLastError = true)]
        private static extern bool CreateEnvironmentBlock(out IntPtr environment, IntPtr token, bool inherit);

        [DllImport("userenv.dll", SetLastError = true)]
        private static extern bool DestroyEnvironmentBlock(IntPtr environment);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);

        #endregion
    }
}
