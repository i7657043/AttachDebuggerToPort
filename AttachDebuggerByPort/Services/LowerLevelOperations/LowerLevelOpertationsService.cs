using EnvDTE;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
using DTEProcess = EnvDTE.Process;
using Process = System.Diagnostics.Process;

namespace AttachDebuggerByPort.Services
{
    public class LowerLevelOpertationsService : ILowerLevelOpertationsService
    {
        [DllImport("ole32.dll")]
        public static extern int CreateBindCtx(int reserved, out IBindCtx ppbc);

        [DllImport("ole32.dll")]
        public static extern int GetRunningObjectTable(int reserved, out IRunningObjectTable prot);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("User32")]
        private static extern int ShowWindow(int hwnd, int nCmdShow);

        private static _DTE VisualStudioInstance;

        private readonly IConsoleWriter _consoleWriter;

        public LowerLevelOpertationsService(IConsoleWriter consoleWriter)
            => _consoleWriter = consoleWriter;

        public bool AttachVisualStudioToProcess(Process debuggerProcess, List<Process> targetProcesses)
        {
            try
            {
                List<DTEProcess> dteProcessesToAttachTo = new List<DTEProcess>();

                if (TryGetVsInstance(debuggerProcess.Id, out VisualStudioInstance))
                {
                    foreach (Process process in targetProcesses)
                    {
                        DTEProcess processToAttachTo = VisualStudioInstance.Debugger.LocalProcesses.Cast<DTEProcess>()
                        .FirstOrDefault(p => p.ProcessID == process.Id);

                        dteProcessesToAttachTo.Add(processToAttachTo);
                    }
                    
                    if (dteProcessesToAttachTo.Count > 0)
                    {
                        foreach (DTEProcess process in dteProcessesToAttachTo)
                        {
                            process.Attach();
                        }

                        ShowWindow((int)debuggerProcess.MainWindowHandle, 3);
                        SetForegroundWindow(debuggerProcess.MainWindowHandle);

                        return true;
                    }
                    else
                    {
                        throw new InvalidOperationException("Visual Studio process cannot find any specified processes to attach to");
                    }
                }
            }
            catch (Exception ex)
            {
                _consoleWriter.PrintCouldNotAttachError();
            }

            return false;
        }

        public bool TryGetVsInstance(int processId, out _DTE instance)
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

        public int GetProcessIdByPortNumber(int portNum)
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
                _consoleWriter.PrintNoProcessesListeningOnSelectedPortError(portNum);
                Environment.Exit(-1);
            }
            catch (Exception)
            {
                _consoleWriter.PrintProcessIdMustBeAnIntegerError();
                Environment.Exit(-1);
            }

            return processId;
        }

        
    }
}
