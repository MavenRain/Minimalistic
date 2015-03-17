using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Minimalistic.Servers;

namespace Minimalistic.Clients
{
	class Program
	{
		static void Main(string[] args)
		{
			(new TestServer(IPAddress.Parse("127.0.0.1"), 8080)).Run();
		}
	}


	public class TestServer : HttpServer
	{
		public TestServer(IPAddress address, int port) : base(address, port) {}

		public override void HandleGetRequest(HttpProcessor p)
		{
			if (p.HttpUrl.Equals("/Fiddle.html"))
			{
				Stream fs = File.Open("Fiddle.html", FileMode.Open);

				p.WriteSuccess();
				//fs.CopyTo (p.OutputStream.BaseStream);
				//p.OutputStream.BaseStream.Flush();
				var text = (new StreamReader(fs, Encoding.UTF8)).ReadToEnd();
				p.OutputStream.Write(text);
				return;
			}

			Console.WriteLine("request: {0}", p.HttpUrl);
			p.WriteSuccess();
			p.OutputStream.WriteLine("<html><body><h1>test server</h1>");
			p.OutputStream.WriteLine("Current Time: " + DateTime.Now.ToString(CultureInfo.InvariantCulture));
			p.OutputStream.WriteLine("url : {0}", p.HttpUrl);

			p.OutputStream.WriteLine("<form method=post action=/form>");
			p.OutputStream.WriteLine("<input type=text name=foo value=foovalue>");
			p.OutputStream.WriteLine("<input type=submit name=bar value=barvalue>");
			p.OutputStream.WriteLine("</form>");
		}

		public override void HandlePostRequest(HttpProcessor p, StreamReader inputData)
		{
			Console.WriteLine("POST request: {0}", p.HttpUrl);
			var data = inputData.ReadToEnd();

			p.WriteSuccess();
			p.OutputStream.WriteLine("<html><body><h1>test server</h1>");
			p.OutputStream.WriteLine("<a href=/test>return</a><p>");
			p.OutputStream.WriteLine("postbody: <pre>{0}</pre>", data);


		}
	}
}
