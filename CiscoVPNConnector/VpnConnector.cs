using System;
using System.Diagnostics;
using System.IO;

namespace CiscoVPNConnecter
{
    public static class VpnConnector
    {
        private static readonly string _vpnCliPath64 = @"C:\Program Files (x86)\Cisco\Cisco AnyConnect Secure Mobility Client\vpncli.exe";
        private static readonly string _vpnCliPath86 = @"C:\Program Files\Cisco\Cisco AnyConnect Secure Mobility Client\vpncli.exe";
        private static readonly string _connectionFile = Environment.GetEnvironmentVariable("TEMP") + "\\vpnConnectionData";
        private static readonly string _vpnCliExe = "vpncli.exe";
        private static readonly string _vpnCliExePath;
        private static readonly string _vpnCliDirPath;
        private static readonly Process _vpncliProcess = new();
        private static readonly ProcessStartInfo _vpncliProcessStartInfo;
        private static string stdOutData = "";

        static VpnConnector()
        {
            _vpnCliExePath = File.Exists(_vpnCliPath86) ? _vpnCliPath86 : _vpnCliPath64;
            _vpnCliDirPath = _vpnCliExePath.Replace(@"\vpncli.exe", "");

            _vpncliProcessStartInfo = new()
            {
                FileName = @"C:\Windows\System32\cmd.exe",
                WorkingDirectory = _vpnCliDirPath,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true
            };
        }

        private static void KillCiscoVpnProcesses()
        {
            Process[] processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                if (process.ProcessName.Contains("vpnui")) process.Kill();
                if (process.ProcessName.Contains("vpncli")) process.Kill();
            }
        }

        public static bool Connect(string host, string userName, string userPassword)
        {
            TextWriter tw = File.CreateText(_connectionFile);
            tw.WriteLine($"connect {host}");
            tw.WriteLine($"{userName}");
            tw.WriteLine($"{userPassword}");
            tw.Close();

            SetArgumentsAndStartProcess($"/C \"{_vpnCliExe} -s < {_connectionFile}\"");
            return IsConnected();
        }

        public static bool Disconnect()
        {
            if (IsConnected()) SetArgumentsAndStartProcess($"/C \"{_vpnCliExe} disconnect\"");
            return !IsConnected();
        }

        public static bool IsConnected()
        {
            SetArgumentsAndStartProcess($"/C \"{_vpnCliExe} state\"");
            if (stdOutData.Contains("state: Disconnected")) return false;
            if (stdOutData.Contains("state: Connected")) return true;
            return false;
        }

        private static void SetArgumentsAndStartProcess(string arguments)
        {
            KillCiscoVpnProcesses();
            _vpncliProcess.Close();
            _vpncliProcessStartInfo.Arguments = arguments;
            _vpncliProcess.StartInfo = _vpncliProcessStartInfo;
            _vpncliProcess.Start();
            _vpncliProcess.WaitForExit();
            ReadOutput();
        }

        private static void ReadOutput()
        {
            stdOutData = _vpncliProcess.StandardOutput.ReadToEnd();
        }
    }
}
