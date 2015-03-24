using System.Windows;
using Minimalistic.Scripting;

namespace Minimalistic.UX.QedLabInstallationAndReporting
{
    /// <summary>
    /// Interaction logic for PackageInstaller.xaml
    /// </summary>
    public partial class PackageInstaller
    {
        public PackageInstaller()
        {
            InitializeComponent();
        }

		void InstallPhonePackages_Click(object sender, RoutedEventArgs e)
		{
			AppManagement.InstallPhoneApp(IpAddress.Text,PackagePaths.Text);
		}

		void InstallStorePackage_Click(object sender, RoutedEventArgs e)
		{
			AppManagement.InstallStoreApp(PackagePath.Text);
		}
	}
}
