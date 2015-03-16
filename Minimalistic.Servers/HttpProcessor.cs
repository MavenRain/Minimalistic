using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Minimalistic.Servers
{
	public class HttpProcessor
	{
		readonly TcpClient socket;
		protected int Port;
		TcpListener listener;

		Stream inputStream;
		StreamWriter outputStream;

		String httpMethod;
		String httpUrl;
		String httpProtocolVersionstring;
		readonly Hashtable httpHeaders = new Hashtable();


		const int MaxPostSize = 10 * 1024 * 1024; // 10MB

		public HttpProcessor(TcpClient s, int port)
		{
			socket = s;
			Port = port;
		}

		void Listen()
		{
			listener = new TcpListener(Dns.GetHostAddresses("localhost").First(), Port);
			listener.Start();
			while (true)
			{
				(new Thread((new HttpProcessor(listener.AcceptTcpClient(), Port)).Process)).Start();
				Thread.Sleep(1);
			}
		}

		public void Run()
		{
			(new Thread(Listen)).Start();
		}

		protected virtual void HandleGetRequest(HttpProcessor p) { throw new NotImplementedException(); }

		protected virtual void HandlePostRequest(HttpProcessor p, StreamReader inputData) { throw new NotImplementedException(); }

		protected virtual void HandlePutRequest(HttpProcessor p, StreamReader inputData) { throw new NotImplementedException(); }

		protected virtual void HandleDeleteRequest(HttpProcessor p, StreamReader inputData) { throw new NotImplementedException(); }

		static string StreamReadLine(Stream inputStream)
		{
			var data = "";
			while (true)
			{
				var nextChar = inputStream.ReadByte();
				if (nextChar == '\n') { break; }
				switch (nextChar)
				{
					case '\r':
						continue;
					case -1:
						Thread.Sleep(1);
						continue;
				}
				data += Convert.ToChar(nextChar);
			}
			return data;
		}
		void Process()
		{
			// we can't use a StreamReader for input, because it buffers up extra data on us inside it's
			// "processed" view of the world, and we want the data raw after the headers
			inputStream = new BufferedStream(socket.GetStream());

			// we probably shouldn't be using a streamwriter for all output from handlers either
			outputStream = new StreamWriter(new BufferedStream(socket.GetStream()));
			try
			{
				ParseRequest();
				ReadHeaders();
				if (httpMethod.Equals("GET"))
				{
					HandleGetRequest();
				}
				else if (httpMethod.Equals("POST"))
				{
					HandlePostRequest();
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Exception: " + e.ToString());
				WriteFailure();
			}
			outputStream.Flush();
			// bs.Flush(); // flush any remaining output
			inputStream = null; outputStream = null; // bs = null;            
			socket.Close();
		}

		void ParseRequest()
		{
			var request = StreamReadLine(inputStream);
			var tokens = request.Split(' ');
			if (tokens.Length != 3)
			{
				throw new Exception("invalid http request line");
			}
			httpMethod = tokens[0].ToUpper();
			httpUrl = tokens[1];
			httpProtocolVersionstring = tokens[2];

			Console.WriteLine("starting: " + request);
		}

		void ReadHeaders()
		{
			Console.WriteLine("readHeaders()");
			String line;
			while ((line = StreamReadLine(inputStream)) != null)
			{
				if (line.Equals(""))
				{
					Console.WriteLine("got headers");
					return;
				}

				var separator = line.IndexOf(':');
				if (separator == -1)
				{
					throw new Exception("invalid http header line: " + line);
				}
				var name = line.Substring(0, separator);
				var pos = separator + 1;
				while ((pos < line.Length) && (line[pos] == ' '))
				{
					pos++; // strip any spaces
				}

				var value = line.Substring(pos, line.Length - pos);
				Console.WriteLine("header: {0}:{1}", name, value);
				httpHeaders[name] = value;
			}
		}

		void HandleGetRequest()
		{
			HandleGetRequest(this);
		}

		const int BufSize = 4096;
		void HandlePostRequest()
		{
			// this post data processing just reads everything into a memory stream.
			// this is fine for smallish things, but for large stuff we should really
			// hand an input stream to the request processor. However, the input stream 
			// we hand him needs to let him see the "end of the stream" at this content 
			// length, because otherwise he won't know when he's seen it all! 

			Console.WriteLine("get post data start");
			var ms = new MemoryStream();
			if (httpHeaders.ContainsKey("Content-Length"))
			{
				var contentLen = Convert.ToInt32(httpHeaders["Content-Length"]);
				if (contentLen > MaxPostSize)
				{
					throw new Exception(
						String.Format("POST Content-Length({0}) too big for this simple server",
						  contentLen));
				}
				var buf = new byte[BufSize];
				var toRead = contentLen;
				while (toRead > 0)
				{
					Console.WriteLine("starting Read, to_read={0}", toRead);

					var numread = inputStream.Read(buf, 0, Math.Min(BufSize, toRead));
					Console.WriteLine("read finished, numread={0}", numread);
					if (numread == 0)
					{
						if (toRead == 0)
						{
							break;
						}
						throw new Exception("client disconnected during post");
					}
					toRead -= numread;
					ms.Write(buf, 0, numread);
				}
				ms.Seek(0, SeekOrigin.Begin);
			}
			Console.WriteLine("get post data end");
			HandlePostRequest(this, new StreamReader(ms));

		}

		void WriteSuccess(string contentType = "text/html")
		{
			// this is the successful HTTP response line
			outputStream.WriteLine("HTTP/1.0 200 OK");
			// these are the HTTP headers...          
			outputStream.WriteLine("Content-Type: " + contentType);
			outputStream.WriteLine("Connection: close");
			// ..add your own headers here if you like

			outputStream.WriteLine("");	// this terminates the HTTP headers.. everything after this is HTTP body..
		}

		void WriteFailure()
		{
			// this is an http 404 failure response
			outputStream.WriteLine("HTTP/1.0 404 File not found");
			// these are the HTTP headers
			outputStream.WriteLine("Connection: close");
			// ..add your own headers here

			outputStream.WriteLine("");	// this terminates the HTTP headers.
		}
	}
}
