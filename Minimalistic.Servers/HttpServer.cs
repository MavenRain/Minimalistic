using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Minimalistic.Servers
{
	public abstract class HttpServer
	{
		protected readonly IPAddress Address;
		protected int Port;
		protected TcpListener Listener;
		

		public HttpServer(IPAddress addr, int port)
		{
			Address = addr;
			Port = port;
		}

		void Listen()
		{
			Listener = new TcpListener(Address, Port);
			Listener.Start();
			while (true)
			{
				(new Thread((new HttpProcessor(Listener.AcceptTcpClient(), this)).Process)).Start();
				Thread.Sleep(1);
			}
		}

		public void Run()
		{
			(new Thread(Listen)).Start();
		}

		public virtual void HandleGetRequest(HttpProcessor p) { throw new NotImplementedException(); }

		public virtual void HandlePostRequest(HttpProcessor p, StreamReader inputData) { throw new NotImplementedException(); }

		public virtual void HandlePutRequest(HttpProcessor p, StreamReader inputData) { throw new NotImplementedException(); }

		public virtual void HandleDeleteRequest(HttpProcessor p, StreamReader inputData) { throw new NotImplementedException(); }
	}
}
