using System;
using System.Diagnostics;
using System.IO;
using IniParser;
using Microsoft.Win32;

namespace Main
{
    static class Program
    {
        static void Main(string[] args)
        {
            var parser = new FileIniDataParser();
            var data = parser.ReadFile("config.ini");
            var path = data["Config"]["Path"];
            BackupUserEnvironment($@"{Path.Combine(path, $"user-env-{DateTime.Now:yyyyMMddHHmmss}.reg")}");
            BackupSystemEnvironment($@"{Path.Combine(path, $"system-env-{DateTime.Now:yyyyMMddHHmmss}.reg")}");
        }

        private static void BackupUserEnvironment(string filepath)
        {
            var path = $@"{Registry.CurrentUser.Name}\Environment";
            BackupRegistry(path, filepath);
        }

        private static void BackupSystemEnvironment(string filepath)
        {
            var path = $@"{Registry.LocalMachine.Name}\SYSTEM\CurrentControlSet\Control\Session Manager\Environment";
            BackupRegistry(path, filepath);
        }

        private static void BackupRegistry(string path, string filepath)
        {
            var dir = Path.GetDirectoryName(filepath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var query = $@"/C REG EXPORT ""{path}"" ""{filepath}"" /y";
            using (var process = new Process())
            {
                var startInfo = new ProcessStartInfo {WindowStyle = ProcessWindowStyle.Hidden, FileName = "cmd.exe", Arguments = query};
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
            }
        }
    }
}