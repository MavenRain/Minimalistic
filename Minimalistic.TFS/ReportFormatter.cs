using System.Text;

namespace Minimalistic.TFS
{
	public static class ReportFormatter
	{
		public static string BugReportEmitter(BugModel bug)
		{
			var reportBuilder = new StringBuilder();
			reportBuilder.Append("<html><head>")
			.Append("<style>")
			.Append(".Segoe { font-family: Segoe UI, Arial, sans-serif; ")
			.Append("margin-left: .5in; ")
			.Append("font-weight: normal; }")
			.Append("</style>")
			.Append("</head>")
			.Append("<body>")
			.Append("<p class='Segoe'>")
			.Append("<b><span>ID: </span></b>")
			.Append(bug.Id.ToString())
			.Append("<b><span><br>State:<span> </span></span></b> ");
			switch (bug.State)
			{
				case State.New:
					reportBuilder.Append("<span style='color:red'>")
					.Append("New")
					.Append("</span>");
					break;
				case State.Resolved:
					reportBuilder.Append("<span style='color:green'>")
					.Append("Resolved")
					.Append("</span>");
					break;
				case State.Unresolved:
					reportBuilder.Append("<span style='color:#C55A11'>")
					.Append("Unresolved")
					.Append("</span>");
					break;
			}
			reportBuilder.Append("<b><span><br>Severity:<span> </span></b> ");
			if (bug.Severity == Severity.High)
			{
				reportBuilder.Append("<span style='color:red'>")
					.Append("High")
					.Append("</span>");
			}
			else
			{
				switch (bug.Severity)
				{
					case Severity.Medium:
						reportBuilder.Append("<span>")
							.Append("Medium")
							.Append("</span>");
						break;
					case Severity.Low:
						reportBuilder.Append("<span>")
							.Append("Low")
							.Append("</span>");
						break;
				}
			}
			reportBuilder.Append("<br><b><span>STARTS Impact:</b><span> </span>")
				.Append(bug.StartsImpact)
				.Append("<b><br>Test Case: </b>")
				.Append(bug.TestCase)
				.Append("</span></p>")
				.Append("<p class='Segoe'><b><span>Summary:</span></b></p>")
				.Append("<p class='Segoe'><span>")
				.Append(bug.Summary.Description)
				.Append("<br>" + bug.Summary.Update)
				.Append("</span></p>")
				.Append("<p class='Segoe'><b><span>Test Steps:</span></b></p>")
				.Append("<ol type='1'>");
			foreach (var step in bug.TestSteps) reportBuilder.Append("<li>" + step + "</li");
			reportBuilder.Append("</ol>");
            reportBuilder.Append("</body>")
			.Append("</html>");
			return reportBuilder.ToString();
		}
	}
}
