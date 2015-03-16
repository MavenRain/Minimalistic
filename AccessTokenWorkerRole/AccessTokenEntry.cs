using Microsoft.WindowsAzure.Storage.Table;

namespace AccessTokenWorkerRole
{
	internal class AccessTokenEntry : TableEntity
	{
		internal string Token { get; set; }
	}
}
