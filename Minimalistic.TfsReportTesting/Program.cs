using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Minimalistic.TFS;

namespace Minimalistic.TfsReportTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            var reportHeader = new ReportHeaderModel
            {
                Company = "company",
                Name = "name",
                ReviewCompletedDate = "reviewCompletedDate",
                ReviewedUsingDevices = "reviewedUsingDevices",
                ReviewingOrganization = "reviewingOrganization",
                Title = "title",
                Version = "version",
                VsoProjectLink = "vsoProjectLink",
                WindowsOsVersion = "windowsOsVersion"
            };
            var bugModel = new BugModel
            {
                ActualResult = "actual result",
                ExpectedResult = "expected result",
                Id = 1234,
                Severity = Severity.High,
                StartsImpact = "Fail",
                State = State.New,
                Summary = new Summary("description", "update"),
                TestCase = new TestCase("description", "device"),
                TestSteps = "FirstTest"
            };
            var bugCollection = new List<WorkItem> { bugModel, bugModel, bugModel, bugModel };
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
                .Append(ReportFormatter.ReportHeadingEmitter(reportHeader))
                .Append(ReportFormatter.BugChartEmitter(bugCollection))
                .Append(ReportFormatter.BugReportEmitter(bugModel))
                .Append(ReportFormatter.EndOfBodyEmitter())
                .Append(ReportFormatter.EndOfDomEmitter());
			//File.AppendAllText(@"D:\Users\v-oniobi\Documents\sample.html", report.ToString());
			//(new TfsCommander(new NetworkCredential("Onyeka.Obi@gmail.com",""), "solomonrain", bugCollection, new List<Project>() )).CommitBugsToServer("TFSTestProject");
	        Console.WriteLine(new TfsCommander(new NetworkCredential("Onyeka.Obi@gmail.com", ""), "solomonrain", bugCollection,
		        new List<Project>()).RetrieveBugsFromProjects(new List<string> {"TFSTestProject"}).ToString());
        }
    }
}
