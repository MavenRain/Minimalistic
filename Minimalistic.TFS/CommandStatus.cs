namespace Minimalistic.TFS
{
	public enum CommandStatusResult
	{
		Success,
		Failure
	}
	public struct CommandStatus
	{
		public CommandStatusResult Result { get; set; }
		public string Message { get; set; }
	}
}
