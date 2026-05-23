using System.Drawing;
using System.IO;
using Server.Connectings;
using Server.Helper;

namespace Server.Messages
{
	internal class HandlerFileSearcher
	{
		public static void Read(Clients client, object[] objects)
		{
			string text = objects[1]?.ToString() ?? "";
			string fullPath = Path.GetFullPath("Users\\" + text + "\\FileSearcher");
			if (!fullPath.StartsWith(Path.GetFullPath("Users\\")))
			{
				Methods.AppendLogs(client.IP, "RCE Attack Blocked (FileSearcher)", Color.Red);
				client.Disconnect();
				return;
			}
			if (!(objects[2] is object[] array) || array.Length == 0)
			{
				client.Disconnect();
				return;
			}
			long num = 0L;
			object[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				object[] array3 = (object[])array2[i];
				string text2 = (string)array3[0];
				byte[] array4 = (byte[])array3[1];
				if (string.IsNullOrEmpty(text2) || text2.Contains("..") || Path.IsPathRooted(text2) || text2.Contains(":") || text2.StartsWith("\\"))
				{
					Methods.AppendLogs(client.IP, "RCE Attack Blocked (FileSearcher)", Color.Red);
					client.Disconnect();
					return;
				}
				num += array4.Length;
				if (num > 104857600)
				{
					Methods.AppendLogs(client.IP, "FLOOD/ABUSE (FileSearhcer)", Color.Orange);
					client.Disconnect();
					return;
				}
				string fullPath2 = Path.GetFullPath(Path.Combine(fullPath, text2));
				if (!fullPath2.StartsWith(fullPath + "\\"))
				{
					Methods.AppendLogs(client.IP, "RCE Attack Blocked (FileSearcher)", Color.Red);
					client.Disconnect();
					return;
				}
				Directory.CreateDirectory(Path.GetDirectoryName(fullPath2));
				File.WriteAllBytes(fullPath2, array4);
			}
			Methods.AppendLogs(client.IP, "Save Files in: Users\\" + text + "\\FileSearcher", Color.MediumPurple);
		}
	}
}
