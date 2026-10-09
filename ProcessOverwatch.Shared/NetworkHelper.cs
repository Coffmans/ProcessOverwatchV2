using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace ProcessOverwatch.Shared
{
    public static class NetworkHelper
    {
        /// <summary>
        /// Environment variable that, when set, overrides the auto-detected public hostname
        /// (useful on machines with multiple network adapters).
        /// </summary>
        public const string PublicHostnameVariable = "PROCESSOVERWATCH_PUBLIC_HOSTNAME";

        /// <summary>
        /// Returns the address Akka.Remote should advertise for this machine. Akka drops inbound
        /// messages whose recipient address doesn't match this value, so it must be the address
        /// other machines use to reach us.
        /// </summary>
        public static string GetPublicHostname()
        {
            var overrideValue = Environment.GetEnvironmentVariable(PublicHostnameVariable);
            if (!string.IsNullOrWhiteSpace(overrideValue))
                return overrideValue.Trim();

            // Ask the OS which local address it would use for outbound traffic. UDP Connect sends no packets.
            try
            {
                using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.Connect("8.8.8.8", 65530);
                if (socket.LocalEndPoint is IPEndPoint endPoint && !IPAddress.IsLoopback(endPoint.Address))
                    return endPoint.Address.ToString();
            }
            catch (SocketException)
            {
                // No default route; fall back to enumerating adapters
            }

            var address = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(n => n.GetIPProperties().UnicastAddresses)
                .Select(a => a.Address)
                .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(a));

            return address?.ToString() ?? "127.0.0.1";
        }
    }
}
