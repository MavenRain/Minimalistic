using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Minimalistic.Servers;

namespace AccessTokenWorkerRole
{
	public class WorkerRole : RoleEntryPoint
	{
		readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
		readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

		public override void Run()
		{
			Trace.TraceInformation("AccessTokenWorkerRole is running");

			//try
			//{
			//	RunAsync(cancellationTokenSource.Token).Wait();
			//}
			//finally
			//{
			//	runCompleteEvent.Set();
			//}

			while (true)
			{
				var vsoAccessTokenServer = new AccessTokenServer(new TcpClient(), 8080)
				{
					StoreAccessToken =
					(accessTokenStorageEndpoint, accessToken) =>
					{
						var table = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString")).CreateCloudTableClient().GetTableReference("access");
						if (table.CreateIfNotExists()) table.Execute(TableOperation.InsertOrReplace(new AccessTokenEntry { Token = accessToken }));
					},
					RetrieveAccessToken = (accessTokenRequestEndpoint, authorizationCode) =>
					{
						RetrieveAccessTokenAsync(accessTokenRequestEndpoint, authorizationCode).ConfigureAwait(true);
					},
					TokenRequestEndpoint = new Uri(CloudConfigurationManager.GetSetting("TokenRequestUrl")),
					TokenEndpoint = new Uri(CloudConfigurationManager.GetSetting("TokenStorageUrl"))
				};
				vsoAccessTokenServer.Run();
			}
		}

		public override bool OnStart()
		{
			// Set the maximum number of concurrent connections
			ServicePointManager.DefaultConnectionLimit = 100;

			// For information on handling configuration changes
			// see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

			var result = base.OnStart();

			Trace.TraceInformation("AccessTokenWorkerRole has been started");

			return result;
		}

		public override void OnStop()
		{
			Trace.TraceInformation("AccessTokenWorkerRole is stopping");

			cancellationTokenSource.Cancel();
			runCompleteEvent.WaitOne();

			base.OnStop();

			Trace.TraceInformation("AccessTokenWorkerRole has stopped");
		}

		static async Task RetrieveAccessTokenAsync(Uri endpoint, string authCode)
		{
			var endpointTail = @"?client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer&client_assertion=" + CloudConfigurationManager.GetSetting("AppSecret") + "&grant_type=urn:ietf:params:oauth:grant-type:jwt-bearer&assertion=" + authCode + "&redirect_uri=" + CloudConfigurationManager.GetSetting("CallbackUrl");
			await (new HttpClient()).PostAsync(endpoint + endpointTail, new ByteArrayContent(new byte[0]));
		}

		//static async Task RunAsync(CancellationToken cancellationToken)
		//{
			// TODO: Replace the following with your own logic.
			//while (!cancellationToken.IsCancellationRequested)
			//{
			//	Trace.TraceInformation("Working");
			//	await Task.Delay(1000, cancellationToken);
			//}
		//}
	}
}
