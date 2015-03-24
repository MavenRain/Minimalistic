namespace Minimalistic.TFS
{
	public enum State
	{
		New = 2,
		Unresolved = 3,
		Resolved = 4
	}

	public enum Severity
	{
		High,
		Medium,
		Low
	}
	public class BugModel
	{
		public int Id { get; set; }
		public State State { get; set; }
		public Severity Severity { get; set; }
		public string StartsImpact { get; set; }
		public TestCase TestCase { get; set; }
		public Summary Summary { get; set; }
		public string TestSteps { get; set; }
		public string ExpectedResult { get; set; }
		public string ActualResult { get; set; }

	}

	public class TestCase
	{
		public TestCase(string description, string device)
		{
			Description = description;
			Device = device;
		}
		public TestCase() {}
		public string Description { get; set; }
		public string Device { get; set; }
	}

	public class Summary
	{
		public Summary(string description, string update)
		{
			Description = description;
			Update = update;
		}

		public Summary() {}
		public string Description { get; set; }
		public string Update { get; set; }
	}
}
