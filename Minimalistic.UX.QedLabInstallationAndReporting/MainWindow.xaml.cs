using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Minimalistic.TFS;

namespace Minimalistic.UX.QedLabInstallationAndReporting
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class BugReporter
    {
	    readonly ObservableCollection<BugModel> stagedBugs; 
        public BugReporter()
        {
            InitializeComponent();
            (new PackageInstaller()).Show();
			stagedBugs = new ObservableCollection<BugModel>();
	        Bugs.ItemsSource = stagedBugs;
        }

		void AddBugToList_Click(object sender, RoutedEventArgs e)
		{
			stagedBugs.Add(new BugModel
			{ ActualResult = ActualBehavior.Text,
				ExpectedResult = ExpectedBehavior.Text,
				Id = Convert.ToInt32(BugId.Text),
				Severity = ParseSeverity(SeveritySelector.Text),
				StartsImpact = StartsStatus.Text,
				State = ParseState(StateSelector.Text),
				Summary = new Summary
				{
					Description = TestCaseSummary.Text,
					Update = TestCaseUpdate.Text
				},
				TestCase = new TestCase
				{
					Description = TestCaseDescription.Text,
					Device = TestCaseDevice.Text
				},
				TestSteps = TestSteps.Text
			});
		}

	    static Severity ParseSeverity(string severity)
	    {
		    switch (severity)
		    {
				case "High":
					return Severity.High;
				case "Medium":
				    return Severity.Medium;
				case "Low":
					return Severity.Low;
				default:
				    return Severity.High;
		    }
	    }

	    static State ParseState(string state)
	    {
		    switch (state)
		    {
				case "New":
					return State.New;
				case "Unresolved":
				    return State.Unresolved;
				case "Resolved":
				    return State.Resolved;
				default:
					return State.New;
		    }
	    }

		void RemoveBugFromList_Click(object sender, RoutedEventArgs e)
		{
			if (Bugs.SelectedItem == null) return;
			stagedBugs.Remove((BugModel) Bugs.SelectedItem);
		}

		void CommitBugList_Click(object sender, RoutedEventArgs e)
		{
			(new TfsCommander()).CommitBugsToServer(stagedBugs,"Onyeka.Obi@gmail.com","","solomonrain", "TFSTestProject");
		}
	}
}
