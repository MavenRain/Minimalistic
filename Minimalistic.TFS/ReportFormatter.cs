using System.Text;

namespace Minimalistic.TFS
{
	public static class ReportFormatter
	{
		public static string BugReportEmitter(BugModel bug)
		{
			var reportBuilder = new StringBuilder();
			reportBuilder.Append("<p class=MsoNormal style='margin-left:.5in'>");
			reportBuilder.Append("<b style='mso-bidi-font-weight:normal'><span style='font - family:\"Segoe UI\", sans - serif'>ID: </span></b>");
			reportBuilder.Append(bug.Id.ToString());
			reportBuilder.Append("<bstyle = 'mso-bidi-font-weight:normal' >< span style = 'font-family:\"Segoe UI\",sans-serif' >< br >State:< span style = 'mso-spacerun:yes' > </ span ></ span ></ b > ");
			switch (bug.State)
			{
				case State.New:
					reportBuilder.Append("");
					break;
				case State.Resolved:
					break;
				case State.Unresolved:
					break;
			}
            return "";
		}
	}
}
