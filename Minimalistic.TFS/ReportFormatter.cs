using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Minimalistic.TFS
{
    public static class ReportFormatter
    {
        public static string SegoeStyleSectionEmitter()
        {
            return ((new StringBuilder())
                .Append(".Segoe { font-family: Segoe UI, Arial, sans-serif; ")
                .Append("margin-left: .5in; ")
                .Append("font-weight: normal; }")).ToString();
        }

        public static string TableGridStyleSectionEmitter()
        {
            return ((new StringBuilder())
                .Append(".TableGrid { border: 1; cellspacing: 0; cellpadding: 0;")
                .Append("margin-left:40.85pt; border-collapse: collapse;")
                .Append("border-style: solid; padding: 0in 5.4pt 0in 5.4pt; }")).ToString();
        }

        public static string TableTextStyleEmitter()
        {
            return ((new StringBuilder())
                .Append("td { vertical-align: top; border-style: solid;")
                .Append("padding: 0in 5.4pt 0in 5.4pt; }")).ToString();
        }

        public static string TableTextFontSizeStyleEmitter()
        {
            return ((new StringBuilder())
                .Append("td p b span { font-size:10.0pt; font-family: Segoe UI, Arial, sans-serif; }")).ToString();
        }

        public static string TableHeadingStyleEmitter()
        {
            return ((new StringBuilder())
                .Append(".TableHeading { background: #BDD6EE; }")).ToString();
        }

        public static string BeginningOfDomEmitter()
        {
            return "<html>";
        }

        public static string EndOfDomEmitter()
        {
            return "</html>";
        }

        public static string BeginningOfHeadEmitter()
        {
            return "<head>";
        }

        public static string EndOfHeadEmitter()
        {
            return "</head>";
        }

        public static string BeginningOfStyleSectionEmitter()
        {
            return "<style>";
        }

        public static string EndOfStyleSectionEmitter()
        {
            return "</style>";
        }

        public static string BeginningOfBodyEmitter()
        {
            return "<body>";
        }

        public static string EndOfBodyEmitter()
        {
            return "</body>";
        }
        //Segoe style class assumed implemented externally
        public static string BugReportEmitter(BugModel bug)
        {
            var reportBuilder = new StringBuilder();
            reportBuilder.Append("<p class='Segoe'>")
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
                .Append(bug.TestCase.Description + " " + bug.TestCase.Device)
                .Append("</span></p>")
                .Append("<p class='Segoe'><b><span>Summary:</span></b></p>")
                .Append("<p class='Segoe'><span>")
                .Append(bug.Summary.Description)
                .Append("<br>" + bug.Summary.Update)
                .Append("</span></p>")
                .Append("<p class='Segoe'><b><span>Test Steps:</span></b></p>")
                .Append("<p class='Segoe'><span>")
				.Append("<br>" + bug.TestSteps)
				.Append("</span></p>")
                .Append("<p class='Segoe'><b><span style='color:black;background:white'>Expected result:</span></b></p>")
                .Append("<p class='Segoe'><span>" + bug.ExpectedResult + "</span></p>")
                .Append("<p class='Segoe'><b><span style='color:black;background:white'>Actual result:</span></b></p>")
                .Append("<p class='Segoe'><span>" + bug.ActualResult + "</span></p>");
            return reportBuilder.ToString();
        }

        public static string ReportHeadingEmitter(ReportHeaderModel reportHeader)
        {
            return ((new StringBuilder())
                .Append("<p class='Segoe'><b><span style='color:#0070C0;text-align:center'>")
                .Append(reportHeader.Title)
                .Append("</span></b>")
                .Append("<br><span style='color:#1F497D;text-align:center'>")
                .Append(reportHeader.ReviewingOrganization)
                .Append("</span>")
                .Append("<p class='Segoe'><b><span>Name:</span></b>")
                .Append(reportHeader.Name + "<br>")
                .Append("<b>Version: </b>")
                .Append(reportHeader.Version + "<br>")
                .Append("<b>Company: </b>")
                .Append(reportHeader.Company + "<br>")
                .Append("<b>VSO Project: </b>")
                .Append("<a href='" + reportHeader.VsoProjectLink + "'>")
                .Append(reportHeader.VsoProjectLink + "</a><br>")
                .Append("<b>Windows OS Version: </b>")
                .Append(reportHeader.WindowsOsVersion + "<br>")
                .Append("<b>Review Completed: </b>")
                .Append(reportHeader.ReviewCompletedDate + "<br>")
                .Append("<b>Reviewed using: </b>")
                .Append(reportHeader.ReviewedUsingDevices + "</p>")).ToString();
        }

        public static string BugChartEmitter(IEnumerable<WorkItem> listOfBugs)
        {
            var bugChart = new StringBuilder();
            var bugs = listOfBugs as WorkItem[] ?? listOfBugs.ToArray();
            bugChart.Append("<table class='TableGrid'>")
                .Append("<tr>")
                .Append("<td class='TableHeading' width=60>")
                .Append("<p style='text-align: center'><b><span>")
                .Append("New")
                .Append("</span></b></p></td>")
                .Append("<td class='TableHeading' width=100>")
                .Append("<p style='text-align: center'><b><span>")
                .Append("Open")
                .Append("</span></b></p></td>")
                .Append("<td class='TableHeading' width=100>")
                .Append("<p style='text-align: center'><b><span>")
                .Append("Closed")
                .Append("</span></b></p></td>")
                .Append("<td class='TableHeading' width=60>")
                .Append("<p style='text-align: center'><b><span>")
                .Append("Total")
                .Append("</span></b></p></td>")
                .Append("</tr>")
                .Append("<tr>")
                .Append("<td width=60>")
                .Append("<p style='text-align: center'><b><span style='color:red'>")
                .Append(bugs.Count(bug => bug.State == "New").ToString())
                .Append("</span></b></p></td>")
                .Append("<td width=100>")
                .Append("<p style='text-align: center'><b><span style='color:#C55A11'>")
                .Append(bugs.Count(bug => bug.State == "Approved").ToString())
                .Append("</span></b></p></td>")
                .Append("<td width=100>")
                .Append("<p style='text-align: center'><b><span style='color:#548235'>")
                .Append(bugs.Count(bug => bug.State == "Done").ToString())
                .Append("</span></b></p></td>")
                .Append("<td width=60>")
                .Append("<p style='text-align: center'><b><span>")
                .Append(bugs.Count().ToString())
                .Append("</span></b></p></td></tr></table>");
            return bugChart.ToString();
        }

        public static string IssuesChartEmitter(IEnumerable<BugModel> listOfBugs)
        {
            return "";
        }
    }
}
