using System.Collections.Generic;

namespace Minimalistic.TFS
{
	public enum State
	{
		New,
		Unresolved,
		Resolved
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
		public List<string> TestSteps { get; set; }
		public string ExpectedResult { get; set; }
		public string ActualResult { get; set; }

	}

	public class TestCase
	{
		public string Description { get; set; }
		public string Device { get; set; }
	}

	public class Summary
	{
		public string Description { get; set; }
		public string Update { get; set; }
	}
}
