using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Minimalistic.Servers
{
	public class AccessTokenProcessor : HttpProcessor
	{
		public AccessTokenProcessor(TcpClient s, int port) : base(s, port) {}

		public override void HandleGetRequest(HttpProcessor processor)
		{
			processor.WriteSuccess();
			processor.OutputStream.Write((new StreamReader(File.OpenRead("AccessToken.bin"), Encoding.UTF8)).ReadToEnd());
		}

		public override void HandlePostRequest(HttpProcessor processor, StreamReader inputData)
		{
			var accessTokenResponse = inputData.ReadToEnd();
			if (accessTokenResponse.Contains("access token"))
			{
				//TODO:  Write access token to AccessToken.bin
				return;
			}
			else
			{
				//TODO:  Call the server endpoint that provides the actual access token with the request code that you just received from accessTokenResponse
			}
		}
	}
}
