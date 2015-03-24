using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Minimalistic.TFS
{
	public class TfsCommander
	{
		public void CommitBugsToServer(IEnumerable<BugModel> discoveredBugs, string username, string password, string vsoAccountName, string projectName)
		{
			//Step 1: Authenticate requester
			var credentials = new TfsClientCredentials(new BasicAuthCredential(new NetworkCredential(username, password)))
			{
				AllowInteractive = false
			};
			var endpoint = new Uri("https://" + vsoAccountName + ".visualstudio.com/DefaultCollection");
			var teamProjectCollection = new TfsTeamProjectCollection(endpoint, credentials);
			teamProjectCollection.Authenticate();

			//Step 2: Generate project work items
			var workItemType = ((teamProjectCollection.GetService<WorkItemStore>()).Projects[projectName]).WorkItemTypes["Bug"];
			foreach (var discoveredBug in discoveredBugs)
			{
				var bug = new WorkItem(workItemType) {Title = discoveredBug.Id.ToString()};
				var report = new StringBuilder();
				report.Append(ReportFormatter.BeginningOfDomEmitter())
					.Append(ReportFormatter.BeginningOfHeadEmitter())
					.Append(ReportFormatter.BeginningOfStyleSectionEmitter())
					.Append(ReportFormatter.SegoeStyleSectionEmitter())
					.Append(ReportFormatter.TableGridStyleSectionEmitter())
					.Append(ReportFormatter.TableHeadingStyleEmitter())
					.Append(ReportFormatter.TableTextStyleEmitter())
					.Append(ReportFormatter.TableTextFontSizeStyleEmitter())
					.Append(ReportFormatter.EndOfStyleSectionEmitter())
					.Append(ReportFormatter.EndOfHeadEmitter())
					.Append(ReportFormatter.BeginningOfBodyEmitter())
					.Append(ReportFormatter.BugReportEmitter(discoveredBug))
					.Append(ReportFormatter.EndOfBodyEmitter())
					.Append(ReportFormatter.EndOfDomEmitter());
				switch (discoveredBug.Severity)
				{
					case Severity.High:
						(bug.Fields.Cast<Field>().Single(f => f.Name == "Severity")).Value = "2 - High";
						break;
					case Severity.Medium:
						(bug.Fields.Cast<Field>().Single(f => f.Name == "Severity")).Value = "3 - Medium";
						break;
					case Severity.Low:
						(bug.Fields.Cast<Field>().Single(f => f.Name == "Severity")).Value = "4 - Low";
						break;
				}
				bug.State = "New";
				(bug.Fields.Cast<Field>().Single(f => f.Name == "Acceptance Criteria")).Value = report.ToString();
				bug.Save();
			}
		}
	}
}
