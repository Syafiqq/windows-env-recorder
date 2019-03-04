using System;
using System.Diagnostics;
using System.IO;
using IniParser;
using IniParser.Model;
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
            var date = $"{DateTime.Now:yyyyMMddHHmmss}";
            //BackupUserEnvironment($@"{Path.Combine(path, $"user-env-{date}.reg")}");
            TransverseUserEnvironment($@"{Path.Combine(path, $"user-env-{date}.ini")}");
            //BackupSystemEnvironment($@"{Path.Combine(path, $"system-env-{date}.reg")}");
        }

        private static void BackupUserEnvironment(string filepath)
        {
            var path = $@"{Registry.CurrentUser.Name}\Environment";
            BackupRegistry(path, filepath);
        }
        
        private static void TransverseUserEnvironment(string filepath)
        {
            void InTransverseUserEnvironment(RegistryKey inKey, IniData inData)
            {
                if (inKey == null) return;
                foreach (var v in inKey.GetValueNames())
                {
                    inData[inKey.Name][v] = (string) inKey.GetValue(v);
                }
                foreach (var v in inKey.GetSubKeyNames())
                {
                    if(v == null) continue;
                    var child = inKey.OpenSubKey(v);
                    InTransverseUserEnvironment(child, inData);
                }
            }
            var data = new IniData();
            var key = Registry.CurrentUser.OpenSubKey("Environment");
            InTransverseUserEnvironment(key, data);
            var parser = new FileIniDataParser();
            parser.WriteFile(filepath, data);
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