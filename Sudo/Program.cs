using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Sudo
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args[0] == "-do")
            {
                var dir = args[1];
                var parentPid = uint.Parse(args[2]);
                var environmentPath = args[3];
                var command = args[4];
                var commandArgs = string.Join(" ", args.Skip(5));

                FreeConsole();
                AttachConsole(parentPid);
                Environment.SetEnvironmentVariable("PATH", environmentPath);

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = command,
                        Arguments = commandArgs,
                        WorkingDirectory = dir,
                        UseShellExecute = false,
                    }
                };

                process.Start();
                process.WaitForExit();
                return process.ExitCode;
            }
            else
            {
                var currentProcess = Process.GetCurrentProcess();
                var workingDirectory = Environment.CurrentDirectory;
                var pid = currentProcess.Id.ToString();
                var environmentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
                var command = args[0];
                var commandArguments = string.Join(" ", args.Skip(1));

                var sudoFilename = currentProcess.MainModule.FileName;
                var sudoArgs = string.Format("-do \"{0}\" {1} \"{2}\" \"{3}\" {4}",
                    workingDirectory, pid,
                    environmentPath,
                    command, commandArguments);

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = sudoFilename,
                        Arguments = sudoArgs,
                        Verb = "runas",
                        WindowStyle = ProcessWindowStyle.Hidden,
                    }
                };

                try
                {
                    process.Start();
                    process.WaitForExit();
                    return process.ExitCode;
                }
                catch (Win32Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return ex.NativeErrorCode;
                }
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool AttachConsole(uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool FreeConsole();
    }
}
