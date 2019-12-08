using System;
using System.Collections.Generic;
using System.Linq;
using Process = System.Diagnostics.Process;

namespace AttachDebuggerByPort.Services
{
    class ApplicationManager : IApplicationManager
    {
        private readonly IConsoleWriter _consoleWriter;
        private readonly ILowerLevelOpertationsService _lowerLevelOpertationsService;

        int portNumberValidated = 0;

        public ApplicationManager(IConsoleWriter consoleWriter, ILowerLevelOpertationsService lowerLevelOpertationsService)
        {
            _consoleWriter = consoleWriter;
            _lowerLevelOpertationsService = lowerLevelOpertationsService;
        }

        public int AttachDebugger(string portNumber)
        {
            try
            {
                portNumberValidated = int.Parse(portNumber);
            }
            catch (Exception)
            {
                _consoleWriter.PrintPortNumberMustBeAnIntegerError();
                throw;
            }

            int targetProcessId = _lowerLevelOpertationsService.GetProcessIdByPortNumber(portNumberValidated);

            Process targetProcess = Process.GetProcessById(targetProcessId);

            _consoleWriter.PrintTargetProcessDetails(targetProcess, portNumberValidated);

            List<Process> vsProcessesOtherThanThisOne = GetVSProcessesOtherThanThisOne();
            if (vsProcessesOtherThanThisOne.Count == 0)
                return -1;

            int vsInstanceChoice = GetBestVsInstanceToAttachAsDebugger(vsProcessesOtherThanThisOne);            

            //AttachVisualStudioToProcess(vsProcessesOtherThanThisOne[vsInstanceChoice], targetProcess);

            _consoleWriter.PrintAttachedSuccess(targetProcess, vsProcessesOtherThanThisOne[vsInstanceChoice], portNumberValidated, vsInstanceChoice);

            _consoleWriter.PrintApplicationsJobCompleteAndExit();

            return 0;
        }

        private int GetBestVsInstanceToAttachAsDebugger(List<Process> vsProcessesOtherThanThisOne)
        {
            List<string> distinctWindowTitles = vsProcessesOtherThanThisOne.Select(x => x.MainWindowTitle).Distinct().ToList();

            //If there is more than 1 variation of VS project open
            if (distinctWindowTitles.Count() > 1)
            {
                for (int i = 0; i < distinctWindowTitles.Count(); i++)
                {
                    _consoleWriter.PrintOtherVsInstanceChoices(distinctWindowTitles, i);
                }

                try
                {
                    //Minus 1 as we increase count of selection on-screen to make it more human readable
                    return int.Parse(Console.ReadLine()) - 1;
                }
                catch (Exception)
                {
                    _consoleWriter.PrintVsInstanceChoiceError();

                    return -1;
                }
            }

            return 0;
        }

        private List<Process> GetVSProcessesOtherThanThisOne()
        {
            List<Process> otherVsProcesses = Process.GetProcesses()
                .Where(o => o.ProcessName.Contains("devenv")
                && !o.MainWindowTitle.Contains("AttachDebuggerByPort")).ToList();

            if (otherVsProcesses.Count == 0)
                _consoleWriter.PrintNoOtherVSInstancesAreOpenToUseAsDebugger();

            return otherVsProcesses;
        }
    }
}
