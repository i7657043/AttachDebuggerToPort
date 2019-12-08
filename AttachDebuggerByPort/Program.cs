using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
using EnvDTE;

using DTEProcess = EnvDTE.Process;
using Process = System.Diagnostics.Process;

namespace JsonObjectCounter
{
    class Program
    {
        [DllImport("ole32.dll")]
        public static extern int CreateBindCtx(int reserved, out IBindCtx ppbc);

        [DllImport("ole32.dll")]
        public static extern int GetRunningObjectTable(int reserved, out IRunningObjectTable prot);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("User32")]
        private static extern int ShowWindow(int hwnd, int nCmdShow);

        static _DTE visualStudioInstance;

        static void Main(string[] args)
        {
            ConsoleColor defaultColour = Console.ForegroundColor;
            defaultColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Attach Debugger By Port\n");
            Console.ForegroundColor = defaultColour;
            
            int vsInstanceChoice = 0;

            int portNum = GetPortNumber();

            int processId = GetProcessId(portNum);

            Process targetProcess = Process.GetProcessById(processId);

            defaultColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nTarget Process {targetProcess.ProcessName} (Id: {processId}) found running on Port {portNum}\n");
            Console.ForegroundColor = defaultColour;

            List<Process> vsProcessesOtherThanThisOne = Process.GetProcesses()
                .Where(o => o.ProcessName.Contains("devenv")
                && !o.MainWindowTitle.Contains("AttachDebuggerByPort")).ToList();

            List<string> distinctWindowTitles = vsProcessesOtherThanThisOne.Select(x => x.MainWindowTitle).Distinct().ToList();

            //If there is more than 1 variation of VS project open
            if (distinctWindowTitles.Count() > 1)
            {
                for (int i = 0; i < distinctWindowTitles.Count(); i++)
                {
                    Console.WriteLine($"Please enter [{i+1}] to attach the VS instance titled the following to the target process:");
                    defaultColour = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(distinctWindowTitles[i] + "\n");
                    Console.ForegroundColor = defaultColour;
                }
            }

            try
            {
                vsInstanceChoice = int.Parse(Console.ReadLine()) - 1;
            }
            catch (Exception)
            {
                defaultColour = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("VS Instance choice must be an Integer.\nPlease try again. Goodbye.");
                Environment.Exit(-1);
            }

            //AttachVisualStudioToProcess(vsProcessesOtherThanThisOne[vsInstanceChoice], targetProcess);

            defaultColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nAttached VS Instance (PID: {vsProcessesOtherThanThisOne[vsInstanceChoice].Id}, " +
                $"Process Name: {vsProcessesOtherThanThisOne[vsInstanceChoice].MainWindowTitle})\n");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"TO\n");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Application Instance (PID: {targetProcess.Id}, " +
                $"Process Name: {targetProcess.MainWindowTitle}-{targetProcess.ProcessName} on Port {portNum})");
            Console.ForegroundColor = defaultColour;

            Console.WriteLine("\n\nPress any key to Exit. The debugger WILL NOT detach.\n");
            Console.ReadKey();
        }

        private static int GetProcessId(int portNum)
        {

            int processId = 0;

            string cmd = $"netstat -aon | findstr {portNum}";

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {cmd}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            process.Start();

            Regex portNumRegex = new Regex($@".*LISTENING(\+?\d+)");

            string result = process.StandardOutput.ReadToEnd().Replace(" ", "").Replace(@"\r", "").Replace(@"\n", "");

            Match match = portNumRegex.Match(result);

            try
            {
                processId = int.Parse(match.Groups[1].Value);
            }
            catch (FormatException)
            {
                ConsoleColor defaultColour = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nNo processes are listening on port {portNum}.\nPlease try again. Goodbye.");
                Console.ForegroundColor = defaultColour;
                Environment.Exit(-1);
            }
            catch (Exception)
            {
                ConsoleColor defaultColour = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nProcess Id must be an Integer.\nPlease try again. Goodbye.");
                Console.ForegroundColor = defaultColour;
                Environment.Exit(-1);
            }

            return processId;
        }

        private static int GetPortNumber()
        {
            Console.WriteLine("Please enter the Port Number:");

            int portNum = 0;

            try
            {
                return int.Parse(Console.ReadLine());
            }
            catch (Exception)
            {
                ConsoleColor defaultColour = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Port number must be an Integer. Please try again. Goodbye.");
                Console.ForegroundColor = defaultColour;
                Environment.Exit(-1);
            }

            return portNum;
        }

        public static void AttachVisualStudioToProcess(Process visualStudioProcess, Process applicationProcess)
        {

            if (TryGetVsInstance(visualStudioProcess.Id, out visualStudioInstance))
            {
                //Find the process you want the Visual Studio instance to attach to...
                DTEProcess processToAttachTo = visualStudioInstance.Debugger.LocalProcesses.Cast<DTEProcess>()
                    .FirstOrDefault(process => process.ProcessID == applicationProcess.Id);

                // Attach to the process.
                if (processToAttachTo != null)
                {
                    processToAttachTo.Attach();

                    ShowWindow((int)visualStudioProcess.MainWindowHandle, 3);
                    SetForegroundWindow(visualStudioProcess.MainWindowHandle);
                }
                else
                {
                    throw new InvalidOperationException("Visual Studio process cannot find specified application '" + applicationProcess.Id + "'");
                }
            }
        }

        private static bool TryGetVsInstance(int processId, out _DTE instance)
        {
            IntPtr numFetched = IntPtr.Zero;
            IRunningObjectTable runningObjectTable;
            IEnumMoniker monikerEnumerator;
            IMoniker[] monikers = new IMoniker[1];

            GetRunningObjectTable(0, out runningObjectTable);
            runningObjectTable.EnumRunning(out monikerEnumerator);
            monikerEnumerator.Reset();

            while (monikerEnumerator.Next(1, monikers, numFetched) == 0)
            {
                IBindCtx ctx;
                CreateBindCtx(0, out ctx);

                string runningObjectName;
                monikers[0].GetDisplayName(ctx, null, out runningObjectName);

                object runningObjectVal;
                runningObjectTable.GetObject(monikers[0], out runningObjectVal);

                if (runningObjectVal is _DTE && runningObjectName.StartsWith("!VisualStudio"))
                {
                    int currentProcessId = int.Parse(runningObjectName.Split(':')[1]);

                    if (currentProcessId == processId)
                    {
                        instance = (_DTE)runningObjectVal;
                        return true;
                    }
                }
            }

            instance = null;
            return false;
        }

    }
}