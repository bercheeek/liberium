using System;
using System.Collections.Generic;
using System.IO;
using Leb128;

namespace Server.Helper
{
    internal class PaleFileProtocol
    {
        public static void Unpack(string basePath, byte[] buff)
        {
            if (buff == null || buff.Length == 0) return;

            object[] array = LEB128.Read(buff);
            int index = 0;

            while (index < array.Length)
            {
                string relativePath = array[index++] as string;
                byte[] fileBytes = array[index++] as byte[];

                if (string.IsNullOrEmpty(relativePath) || fileBytes == null || fileBytes.Length == 0)
                    continue;

                if (relativePath.Contains("..") ||
                    relativePath.StartsWith("/") ||
                    relativePath.StartsWith("\\") ||
                    relativePath.Contains(":") ||
                    Path.IsPathRooted(relativePath) ||
                    string.IsNullOrWhiteSpace(relativePath))
                {
                    continue;
                }

                string fullPath;
                try
                {
                    fullPath = Path.GetFullPath(Path.Combine(basePath, relativePath));
                }
                catch
                {
                    continue;
                }

                string normalizedBase = Path.GetFullPath(basePath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

                if (!fullPath.StartsWith(normalizedBase, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    string dir = Path.GetDirectoryName(fullPath);
                    if (!string.IsNullOrEmpty(dir))
                        Directory.CreateDirectory(dir);

                    File.WriteAllBytes(fullPath, fileBytes);
                }
                catch { }
            }
        }

        public static byte[] Pack(string path)
        {
            List<object> list = new List<object>();

            if (!Directory.Exists(path))
                return LEB128.Write(list.ToArray());

            string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                try
                {
                    string relPath = file.Substring(path.Length).TrimStart('\\', '/');

                    if (string.IsNullOrEmpty(relPath) || relPath.Contains("..") || Path.IsPathRooted(relPath))
                        continue;

                    byte[] bytes = File.ReadAllBytes(file);
                    list.Add(relPath);
                    list.Add(bytes);
                }
                catch { }
            }

            return LEB128.Write(list.ToArray());
        }
    }
}
