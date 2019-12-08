using EnvDTE;
using System;
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

        private static _DTE visualStudioInstance;

        private readonly IConsoleWriter _consoleWriter;

        public LowerLevelOpertationsService(IConsoleWriter consoleWriter)
            => _consoleWriter = consoleWriter;

        public bool AttachVisualStudioToProcess(Process visualStudioProcess, Process applicationProcess)
        {
            try
            {
                if (TryGetVsInstance(visualStudioProcess.Id, out visualStudioInstance))
                {
                    DTEProcess processToAttachTo = visualStudioInstance.Debugger.LocalProcesses.Cast<DTEProcess>()
                        .FirstOrDefault(process => process.ProcessID == applicationProcess.Id);

                    if (processToAttachTo != null)
                    {
                        processToAttachTo.Attach();

                        ShowWindow((int)visualStudioProcess.MainWindowHandle, 3);
                        SetForegroundWindow(visualStudioProcess.MainWindowHandle);

                        return true;
                    }
                    else
                    {
                        throw new InvalidOperationException("Visual Studio process cannot find specified application '" + applicationProcess.Id + "'");
                    }
                }
            }
            catch (Exception)
            {
                _consoleWriter.PrintCouldNotAttachError(applicationProcess);
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
