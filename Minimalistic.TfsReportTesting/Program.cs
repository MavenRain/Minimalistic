using System.Collections.Generic;
using System.IO;
using System.Text;
using Minimalistic.TFS;

namespace Minimalistic.TfsReportTesting
{
	class Program
	{
		static void Main(string[] args)
		{
			var bugModel = new BugModel
			{
				ActualResult = "actual result",
				ExpectedResult = "expected result",
				Id = 1234,
				Severity = Severity.High,
				StartsImpact = "Fail",
				State = State.New,
				Summary = new Summary("description","update"),
				TestCase = new TestCase("description","device"),
				TestSteps = new List<string> { "FirstTest", "SecondTest", "ThirdTest"}
			};
			var report = new StringBuilder();
			report.Append(ReportFormatter.BeginningOfDomEmitter())
				.Append(ReportFormatter.BeginningOfHeadEmitter())
				.Append(ReportFormatter.BeginningOfStyleSectionEmitter())
				.Append(ReportFormatter.SegoeStyleSectionEmitter())
				.Append(ReportFormatter.EndOfStyleSectionEmitter())
				.Append(ReportFormatter.EndOfHeadEmitter())
				.Append(ReportFormatter.BeginningOfBodyEmitter())
				.Append(ReportFormatter.BugReportEmitter(bugModel))
				.Append(ReportFormatter.EndOfBodyEmitter())
				.Append(ReportFormatter.EndOfDomEmitter());
			File.AppendAllText(@"D:\Users\v-oniobi\Documents\sample.html", report.ToString());
		}
	}
}
