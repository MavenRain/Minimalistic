using System.Net.Sockets;

namespace Minimalistic.Servers
{
	public class AccessTokenServer : HttpProcessor
	{
		public AccessTokenServer(TcpClient socket, int port) : base(socket, port) {}

		protected sealed override void HandleGetRequest(HttpProcessor httpProcessor)
		{
			
		}
	}
}
