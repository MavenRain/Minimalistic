using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace Minimalistic.Servers
{
	public class WebServer
	{
		readonly HttpListener listener = new HttpListener();
		readonly Func<HttpListenerRequest, string> responderMethod;

		public WebServer(IReadOnlyCollection<string> prefixes, Func<HttpListenerRequest, string> method)
		{
			if (!HttpListener.IsSupported)
				throw new NotSupportedException(
					"Needs Windows XP SP2, Server 2003 or later.");

			// URI prefixes are required, for example 
			// "http://localhost:8080/index/".
			if (prefixes == null || prefixes.Count == 0)
				throw new ArgumentException("prefixes");

			// A responder method is required
			if (method == null)
				throw new ArgumentException("method");

			foreach (var s in prefixes)
				listener.Prefixes.Add(s);

			responderMethod = method;
			listener.Start();
		}

		public WebServer(Func<HttpListenerRequest, string> method, params string[] prefixes)
			: this(prefixes, method)
		{ }

		public void Run()
		{
			ThreadPool.QueueUserWorkItem(o =>
			{
				Console.WriteLine("Webserver running...");
				while (listener.IsListening)
				{
					try
					{
						ThreadPool.QueueUserWorkItem(c =>
						{
							var ctx = c as HttpListenerContext;
							if (ctx == null)
							{
								Console.WriteLine("Http listener context was not properly retrieved");
								return;
							}

							var rstr = responderMethod(ctx.Request);
							var buf = Encoding.UTF8.GetBytes(rstr);
							ctx.Response.ContentLength64 = buf.Length;
							ctx.Response.OutputStream.Write(buf, 0, buf.Length);
							ctx.Response.OutputStream.Close();

						}, listener.GetContext());
					}
					catch (HttpListenerException)
					{
						//Server likely terminated normally
					}
				}
			});
		}

		public void Stop()
		{
			listener.Stop();
			listener.Close();
		}
	}
}

