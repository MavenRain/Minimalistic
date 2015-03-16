using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace Minimalistic.Servers
{
	public class AccessTokenServer : HttpProcessor
	{
		public Uri TokenEndpoint { get; set; }

		public Uri TokenRequestEndpoint { get; set; }

		public delegate void AccessTokenStorageRequest(Uri accessTokenStorageEndpoint, string accessToken);

		public delegate void AccessTokenRetrievalRequest(Uri accessTokenRequestEndpoint, string authorizationCode);

		public AccessTokenStorageRequest StoreAccessToken { get; set; }

		public AccessTokenRetrievalRequest RetrieveAccessToken { get; set; }
		public AccessTokenServer(TcpClient socket, int port) : base(socket, port) {}

		protected sealed override void HandleGetRequest(HttpProcessor httpProcessor)
		{
			HandleGetRequestAsync(httpProcessor).ConfigureAwait(true);
		}

		async Task HandleGetRequestAsync(HttpProcessor httpProcessor)
		{
			httpProcessor.WriteSuccess();
			httpProcessor.OutputStream.Write(await (await (new HttpClient()).GetAsync(TokenEndpoint)).Content.ReadAsStringAsync());
		}

		protected sealed override void HandlePostRequest(HttpProcessor httpProcessor, StreamReader inputData)
		{
			var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(inputData.ReadToEnd());
			if (response.ContainsKey("access_token"))
			{
				StoreAccessToken(TokenEndpoint,response["access_token"]);
				return;
			}
			RetrieveAccessToken(TokenRequestEndpoint,HttpUtility.ParseQueryString((new Uri(HttpUrl)).Query)["code"]);
		}
	}
}
