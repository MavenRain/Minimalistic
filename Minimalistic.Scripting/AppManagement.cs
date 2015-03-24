using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Management.Automation;

namespace Minimalistic.Scripting
{
    public static class AppManagement
    {
        public static Collection<PSObject> UserModelIDs()
        {
            const string script = @"foreach($app in Get-AppxPackage) { foreach ($id in (Get-AppxPackageManifest $app).package.applications.application.id) { $app.PackageFamilyName + '!' + $id } }";
            return PowerShell.Create().AddScript(script).Invoke();
        }

        public static Collection<PSObject> PackageNames()
        {
            const string script = @"foreach($app in Get-AppxPackage) { foreach ($id in (Get-AppxPackageManifest $app).package.applications.application.id) { $app.PackageFullName } }";
            return PowerShell.Create().AddScript(script).Invoke();
        }

        public static void InstallPhoneApp(string ip, string packagePaths)
        {
            (new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "C:\\Windows\\SysWOW64\\WindowsPowerShell\\v1.0\\powershell.exe",
                    Arguments = " -Command \"Open-Device " + ip + "; Deploy-Device -Packages " + packagePaths + "\""
                }
            }).Start();
        }

        public static void InstallStoreApp(string path)
        {
            var script = "Add-AppxPackage " + path;
            PowerShell.Create().AddScript(script).Invoke();
        }
    }
}
