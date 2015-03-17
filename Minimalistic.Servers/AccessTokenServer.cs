using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace Minimalistic.Servers
{
	public class AccessTokenServer : HttpServer
	{
		public Uri TokenEndpoint { get; set; }

		public Uri TokenRequestEndpoint { get; set; }

		public delegate void AccessTokenStorageRequest(Uri accessTokenStorageEndpoint, string accessToken);

		public delegate void AccessTokenRetrievalRequest(Uri accessTokenRequestEndpoint, string authorizationCode);

		public AccessTokenStorageRequest StoreAccessToken { get; set; }

		public AccessTokenRetrievalRequest RetrieveAccessToken { get; set; }

		public AccessTokenServer(IPAddress address, int port) : base(address, port)
		{
			StoreAccessToken = (storageEndpoint, accessToken) => { throw new NotImplementedException(); };
			RetrieveAccessToken = (requestEndpoint, authorizationCode) => { throw new NotImplementedException(); };
		}

		public sealed override void HandleGetRequest(HttpProcessor httpProcessor)
		{
			HandleGetRequestAsync(httpProcessor).ConfigureAwait(true);
		}

		async Task HandleGetRequestAsync(HttpProcessor httpProcessor)
		{
			httpProcessor.WriteSuccess();
			if (HttpUtility.ParseQueryString((new Uri(httpProcessor.HttpUrl)).Query).Count > 0)
			{
				RetrieveAccessToken(TokenRequestEndpoint, HttpUtility.ParseQueryString((new Uri(httpProcessor.HttpUrl)).Query)["code"]);
				return;
			}
			httpProcessor.OutputStream.Write(await (await (new HttpClient()).GetAsync(TokenEndpoint)).Content.ReadAsStringAsync());
		}

		public sealed override void HandlePostRequest(HttpProcessor httpProcessor, StreamReader inputData)
		{
			StoreAccessToken(TokenEndpoint, JsonConvert.DeserializeObject<Dictionary<string, string>>(inputData.ReadToEnd())["access_token"]);
		}
	}
}
