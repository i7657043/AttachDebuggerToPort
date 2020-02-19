using AttachDebuggerByPort.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Process = System.Diagnostics.Process;

namespace AttachDebuggerByPort.Services
{
    class ApplicationManager : IApplicationManager
    {
        private readonly IConsoleWriter _consoleWriter;
        private readonly ILowerLevelOpertationsService _lowerLevelOpertationsService;

        public ApplicationManager(IConsoleWriter consoleWriter, ILowerLevelOpertationsService lowerLevelOpertationsService)
        {
            _consoleWriter = consoleWriter;
            _lowerLevelOpertationsService = lowerLevelOpertationsService;
        }

        public int AttachDebugger(List<string> portNumbers, string filter)
        {
            List<int> portNumbersParsed = ParsePortNumbers(portNumbers);
            if (portNumbersParsed == null)
                return -1;

            List<Process> vsProcessesOtherThanThisOne = GetVSProcessesOtherThanThisOne()?.FilterProcesses(filter);
            if (vsProcessesOtherThanThisOne.Count == 0)
            {
                _consoleWriter.PrintNoOtherVSInstancesAreOpenToUseAsDebugger();
                return -1;
            }   

            List<Process> targetProcesses = new List<Process>();

            foreach (int port in portNumbersParsed)
            {
                int targetProcessId = _lowerLevelOpertationsService.GetProcessIdByPortNumber(port);

                Process targetProcess = Process.GetProcessById(targetProcessId);

                _consoleWriter.PrintTargetProcessDetails(targetProcess, port);

                targetProcesses.Add(targetProcess);
            }

            Process vsInstanceToAttachTo = vsProcessesOtherThanThisOne.Count > 1
                ? GetBestVsInstanceToAttachAsDebugger(vsProcessesOtherThanThisOne)
                : vsProcessesOtherThanThisOne[0];

            bool attached = _lowerLevelOpertationsService.AttachVisualStudioToProcess(vsInstanceToAttachTo, targetProcesses);
            if (!attached)
                return -1;

            _consoleWriter.PrintAttachedSuccess(targetProcesses, vsInstanceToAttachTo);

            _consoleWriter.PrintApplicationsJobCompleteAndExit();

            return 0;
        } 

        private List<int> ParsePortNumbers(List<string> portNumbers)
        {
            try
            {
                List<int> portNumbersParsed = new List<int>();

                foreach (string port in portNumbers)
                {
                    portNumbersParsed.Add(int.Parse(port));
                }

                return portNumbersParsed;
            }
            catch (Exception)
            {
                _consoleWriter.PrintPortNumberMustBeAnIntegerError();
            }

            return null;
        }

        private Process GetBestVsInstanceToAttachAsDebugger(List<Process> vsProcessesOtherThanThisOne)
        {
            List<string> vsWindows = vsProcessesOtherThanThisOne.Select(x => x.MainWindowTitle.Replace("(Administrator)", string.Empty).Trim())
                .Where(mainWindowTitle => !mainWindowTitle.Contains("Running") && !mainWindowTitle.Contains("Debug"))
                .Distinct()
                .ToList();

            //If there are more than 1 different VS Windows open
            if (vsWindows.Count() > 1)
            {
                for (int i = 0; i < vsWindows.Count(); i++)
                {
                    _consoleWriter.PrintOtherVsInstanceChoices(vsWindows, i);
                }

                _consoleWriter.PrintGetVsInstanceChoice();

                try
                {
                    //+1 above Index for human-readable std output
                    int choice = int.Parse(Console.ReadLine()) - 1;

                    Process processChoice = vsProcessesOtherThanThisOne.Where(x => x.MainWindowTitle.Contains(vsWindows[choice]))
                        .FirstOrDefault(x => x.MainWindowTitle.Contains("Administrator"))
                        //If more than 1 exists and we can't prioritise by Admin then get newest running instance
                        ?? vsProcessesOtherThanThisOne.OrderBy(x => x.StartTime)
                        .FirstOrDefault(x => x.MainWindowTitle.Contains(vsWindows[choice]) 
                        && !x.MainWindowTitle.Contains("Running") && !x.MainWindowTitle.Contains("Debug"));

                    return vsProcessesOtherThanThisOne[choice];
                }
                catch (Exception)
                {
                    _consoleWriter.PrintVsInstanceChoiceError();

                    return null;
                }
            }

            //Prioritise attaching debugger to VS instance running as Admin or return any VS instance not already in Debug or Running mode
            return vsProcessesOtherThanThisOne.FirstOrDefault(x => x.MainWindowTitle.Contains("Administrator")) 
                ?? vsProcessesOtherThanThisOne.FirstOrDefault(x => !x.MainWindowTitle.Contains("Running") && !x.MainWindowTitle.Contains("Debug"));
        }
        
        private List<Process> GetVSProcessesOtherThanThisOne()
        {
            List<Process> otherVsProcesses = Process.GetProcesses()
                .Where(o => o.ProcessName.Contains("devenv")
                //Don't attach to VS debugging the AttachDebuggerByPort app (For testing and debugging this app)
                && !o.MainWindowTitle.Contains("AttachDebuggerByPort")).ToList();        

            return otherVsProcesses;
        }
    }
}
