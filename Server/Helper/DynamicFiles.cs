using System;
using System.IO;

namespace Server.Helper
{
    internal class DynamicFiles
    {
        public static void Save(string basePath, object[] dynamicFiles)
        {
            string trustedBase = Path.GetFullPath(basePath);

            int i = 0;
            while (i < dynamicFiles.Length)
            {
                object[] fileData = (object[])dynamicFiles[i++];
                string relativePath = (string)fileData[0];
                byte[] bytes = (byte[])fileData[1];

                if (string.IsNullOrEmpty(relativePath) ||
                    relativePath.Contains("..") ||
                    relativePath.Contains(":") ||
                    Path.IsPathRooted(relativePath) ||
                    relativePath.StartsWith("\\") ||
                    relativePath.Contains("/") ||
                    relativePath.Contains(@"\\"))
                {
                    return;
                }

                string fullPath;
                try
                {
                    fullPath = Path.GetFullPath(Path.Combine(trustedBase, relativePath));
                }
                catch
                {
                    return;
                }

                if (!fullPath.StartsWith(trustedBase, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                try
                {
                    string dir = Path.GetDirectoryName(fullPath);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    File.WriteAllBytes(fullPath, bytes);
                }
                catch
                {
                }
            }
        }
    }
}
