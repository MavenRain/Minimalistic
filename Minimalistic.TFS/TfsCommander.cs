using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Minimalistic.TFS
{
	public class TfsCommander
	{
		readonly IEnumerable<BugModel> bugList;
		IEnumerable<Project> projectList;
		readonly TfsTeamProjectCollection teamProjectCollection;

		public TfsCommander(ICredentials credential, string serverName, IEnumerable<BugModel> bugModel,
			IEnumerable<Project> projects)
		{
			bugList = bugModel;
			projectList = projects;

			//Heavily loaded constructor, but useless object results if authorization is unsuccessful
			var tfsCredentials = new TfsClientCredentials(new BasicAuthCredential(credential))
			{
				AllowInteractive = false
			};
			var endpoint = new Uri("https://" + serverName + ".visualstudio.com/DefaultCollection");
			teamProjectCollection = new TfsTeamProjectCollection(endpoint, tfsCredentials);
			teamProjectCollection.Authenticate();
		}
		public CommandStatus CommitBugsToServer(string projectName)
		{
			var workItemType = ((teamProjectCollection.GetService<WorkItemStore>()).Projects[projectName]).WorkItemTypes["Bug"];
			foreach (var discoveredBug in bugList)
			{
				var bug = new WorkItem(workItemType) {Title = discoveredBug.Summary.Description};
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
				if (bug.Validate().Count > 0)
					return new CommandStatus
					{
						Result = CommandStatusResult.Failure,
						Message = "The following work item fields failed validation: " + bug.Validate().Cast<Field>()
					};
				bug.Save();	
			}
			return new CommandStatus() { Result = CommandStatusResult.Success, Message = "Save completed" };
		}

	    public CommandStatus CommitReportToServer(string projectName)
	    {
	        var bug =
	            new WorkItem(
	                ((teamProjectCollection.GetService<WorkItemStore>()).Projects[projectName]).WorkItemTypes["Bug"]);
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
			    .Append(ReportFormatter.BeginningOfBodyEmitter());
		    foreach (var discoveredBug in bugList) report.Append(ReportFormatter.BugReportEmitter(discoveredBug));
		    report.Append(ReportFormatter.EndOfBodyEmitter())
			    .Append(ReportFormatter.EndOfDomEmitter());
			bug.State = "New";
			(bug.Fields.Cast<Field>().Single(f => f.Name == "Severity")).Value = "2 - High";
			(bug.Fields.Cast<Field>().Single(f => f.Name == "Acceptance Criteria")).Value = report.ToString();
			if (bug.Validate().Count > 0)
				return new CommandStatus
				{
					Result = CommandStatusResult.Failure,
					Message = "The following work item fields failed validation: " + bug.Validate().Cast<Field>()
				};
			bug.Save();
			return new CommandStatus() { Result = CommandStatusResult.Success, Message = "Save completed" };
		}

		public List<WorkItem> RetrieveBugsFromProjects(IEnumerable<string> projectNames)
		{
			var workItemStore = teamProjectCollection.GetService<WorkItemStore>();
            var selectedProjects = workItemStore.Projects.Cast<Project>().Where(project => projectNames.Contains(project.Name));
			var bugList = new List<WorkItem>();
			foreach (var workItemCollection in selectedProjects.Select(selectedProject => workItemStore.Query("select * from WorkItems where [Area Path] = '" + selectedProject.Name + "' and [Work Item Type] = 'Bug'")))	
			{
				bugList.AddRange(workItemCollection.Cast<WorkItem>());
			}
			return bugList;
		}
	}
}
