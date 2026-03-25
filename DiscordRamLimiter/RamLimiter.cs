using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiscordRamLimiter
{
    internal class RamLimiter
    {
        [DllImport("kernel32.dll")]
        static extern bool SetProcessWorkingSetSize(IntPtr proc, int min, int max);
        static int min = -1;
        static int max = -1;


        private static void runOnProcess(Process[] processes, bool active)
        {
            int DiscordId = -1;
            long workingSet = 0;
            foreach (Process discord in processes)
            {
                if (discord.WorkingSet64 > workingSet)
                {
                    workingSet = discord.WorkingSet64;
                    DiscordId = discord.Id;
                }
            }
            while (DiscordId != -1 && active == true)
            {
                if (DiscordId != -1 && active == true)
                {
                    GC.Collect(); // Force garbage collection
                    GC.WaitForPendingFinalizers(); // Wait for all finalizers to complete before continuing. 
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT) // Check OS version platform 
                    {
                        SetProcessWorkingSetSize(Process.GetProcessById(DiscordId).Handle, min, max);
                    }
                    var wmiObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");
                    var memoryValues = wmiObject.Get().Cast<ManagementObject>().Select(mo => new {
                        FreePhysicalMemory = Double.Parse(mo["FreePhysicalMemory"].ToString()),
                        TotalVisibleMemorySize = Double.Parse(mo["TotalVisibleMemorySize"].ToString())
                    }).FirstOrDefault();
                    if (memoryValues != null)
                    {
                        var percent = ((memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory) / memoryValues.TotalVisibleMemorySize) * 100;
                        Thread.Sleep(300);
                    }
                    Thread.Sleep(1);
                }
            }
        }

        public static void start(bool active)
        {
            Process[] discordProcesses = Process.GetProcessesByName("Discord");
            Process[] discordCanaryProcesses = Process.GetProcessesByName("DiscordCanary");
            Process[] discordPTBProcesses = Process.GetProcessesByName("DiscordPTB");

            // Discord
            Thread thr1 = new Thread(() => runOnProcess(discordProcesses, active));
            thr1.Name = "RamLimiterThread";
            thr1.IsBackground = true;
            thr1.Start();

            // Discord Canary
            Thread thr2 = new Thread(() => runOnProcess(discordCanaryProcesses, active));
            thr2.Name = "RamLimiterThread";
            thr2.IsBackground = true;
            thr2.Start();


            // Discord PTB
            Thread thr3 = new Thread(() => runOnProcess(discordPTBProcesses, active));
            thr3.Name = "RamLimiterThread";
            thr3.IsBackground = true;
            thr3.Start();
        }
    }
}
