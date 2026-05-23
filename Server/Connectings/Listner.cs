using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using Server.Helper;

namespace Server.Connectings
{
	public class Listner
	{
		private Socket Server { get; set; }
		public int port { get; set; }
		
		private static readonly ConcurrentDictionary<string, int> ipConnections = new ConcurrentDictionary<string, int>();
		private const int MAX_CONNECTIONS_PER_IP = 3;

		public void Stop()
		{
			Socket server = this.Server;
			if (server != null)
			{
				server.Dispose();
			}
			Methods.AppendLogs("Server", "Stop Listner: " + this.port.ToString(), Color.Red);
		}

		public Listner(int port)
		{
			this.port = port;
			IPEndPoint localEP = new IPEndPoint(IPAddress.Any, port);
			this.Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			this.Server.ReceiveBufferSize = 512000;
			this.Server.SendBufferSize = 512000;
			this.Server.Bind(localEP);
			this.Server.Listen(2500);
			this.Server.BeginAccept(new AsyncCallback(this.EndAccept), null);
			Methods.AppendLogs("Server", "Start Listner: " + port.ToString(), Color.Green);
		}

		private void EndAccept(IAsyncResult ar)
		{
			try
			{
				Socket clientSocket = this.Server.EndAccept(ar);
				string clientIP = clientSocket.RemoteEndPoint.ToString().Split(':')[0];
				
				if (clientIP != "127.0.0.1")
				{
					int currentConnections = ipConnections.GetOrAdd(clientIP, 0);
					
					if (currentConnections >= MAX_CONNECTIONS_PER_IP)
					{
						clientSocket.Close();
						clientSocket.Dispose();
						this.Server.BeginAccept(new AsyncCallback(this.EndAccept), null);
						return;
					}
					
					ipConnections.AddOrUpdate(clientIP, 1, (key, oldValue) => oldValue + 1);
					
					Clients client = new Clients(clientSocket);
					client.eventDisconnect += (s, e) => 
					{
						ipConnections.AddOrUpdate(clientIP, 0, (key, oldValue) => oldValue > 0 ? oldValue - 1 : 0);
					};
				}
				else
				{
					new Clients(clientSocket);
				}
			}
			catch
			{
			}
			try
			{
				this.Server.BeginAccept(new AsyncCallback(this.EndAccept), null);
			}
			catch
			{
			}
		}
	}
}
