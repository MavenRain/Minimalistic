using System.Text;

namespace Minimalistic.TFS
{
	public static class ReportFormatter
	{
		public static string BugReportEmitter(BugModel bug)
		{
			var reportBuilder = new StringBuilder();
			reportBuilder.Append("<html><head>");
			reportBuilder.Append("<style>");
			reportBuilder.Append(".Segoe { font-family: Segoe UI, Arial, sans-serif; ");
			reportBuilder.Append("margin-left: .5in; ");
			reportBuilder.Append("font-weight: normal; }");
			reportBuilder.Append("</style>");
			reportBuilder.Append("</head>");
			reportBuilder.Append("<body>");
			reportBuilder.Append("<p class='Segoe'>");
			reportBuilder.Append("<b><span>ID: </span></b>");
			reportBuilder.Append(bug.Id.ToString());
			reportBuilder.Append("<b><span><br>State:<span> </span></span></b> ");
			switch (bug.State)
			{
				case State.New:
					reportBuilder.Append("<span style='color:red'>");
					reportBuilder.Append("New");
					reportBuilder.Append("</span>");
					break;
				case State.Resolved:
					reportBuilder.Append("<span style='color:green'>");
					reportBuilder.Append("Resolved");
					reportBuilder.Append("</span>");
					break;
				case State.Unresolved:
					reportBuilder.Append("<span style='color:#C55A11'>");
					reportBuilder.Append("Unresolved");
					reportBuilder.Append("</span>");
					break;
			}
			reportBuilder.Append("<b><span><br>Severity:<span> </span></span></b> ");
			reportBuilder.Append("</body>");
			reportBuilder.Append("</html>");
			return reportBuilder.ToString();
		}
	}
}
