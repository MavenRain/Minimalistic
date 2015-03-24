using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Minimalistic.Servers;

namespace Minimalistic.Clients
{
	class Program
	{
		static void Main(string[] args)
		{
			//(new TestServer(IPAddress.Parse("127.0.0.1"), 8080)).Run();
			var ws = new WebServer(SendResponse, "http://localhost:8080/test/");
			ws.Run();
			Console.WriteLine("A simple webserver.  Press a key to quit.");
			Console.ReadKey();
			ws.Stop();
		}

		public static string SendResponse(HttpListenerRequest request)
		{
			return string.Format("<html><body>My web page.<br>{0}</body></html>", DateTime.Now);
		}

		public static string GetAccessToken(HttpListenerRequest request)
		{
			var response = (new HttpClient()).GetAsync(
				new Uri("https://app.vssps.visualstudio.com/oauth2/authorize?client_id=CAF90BC0-D1ED-4762-B40A-3CDCD8EF4F84&response_type=Assertion&state=User1&scope=vso.build_execute%20vso.chat_manage%20vso.code_manager%20vso.test_write%20vso.work_write&redirect_uri=https://vsoaccesstoken.cloudapp.net/test/"));
			return "";
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
