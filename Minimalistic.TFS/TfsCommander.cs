using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Minimalistic.TFS
{
	public class TfsCommander
	{
		public void CommitBugsToServer(List<BugModel> discoveredBugs, string username, string password, string vsoAccountName, string projectName)
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
				var bug = new WorkItem(workItemType) {Title = Path.GetRandomFileName()};
				(bug.Fields.Cast<Field>().Single(f => f.Name == "Acceptance Criteria")).Value = discoveredBug.Summary.Description + "\r\n" + discoveredBug.ExpectedResult + "\r\n" + discoveredBug.ActualResult;
				bug.Save();
			}
		}
	}
}
