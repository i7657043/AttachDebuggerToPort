﻿using AttachDebuggerByPort.Extensions;
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
        private Process vsInstanceToAttachTo;

        public ApplicationManager(IConsoleWriter consoleWriter, ILowerLevelOpertationsService lowerLevelOpertationsService)
        {
            _consoleWriter = consoleWriter;
            _lowerLevelOpertationsService = lowerLevelOpertationsService;
        }

        public int AttachDebugger(string portNumber, string filter)
        {
            int portNumberParsed = ParsePortNumber(portNumber);

            int targetProcessId = _lowerLevelOpertationsService.GetProcessIdByPortNumber(portNumberParsed);

            Process targetProcess = Process.GetProcessById(targetProcessId);

            _consoleWriter.PrintTargetProcessDetails(targetProcess, portNumberParsed);

            List<Process> vsProcessesOtherThanThisOne = GetVSProcessesOtherThanThisOne().FilterProcesses(filter);

            if (vsProcessesOtherThanThisOne.Count == 0)
                return -1;
            else if (vsProcessesOtherThanThisOne.Count > 1)
                vsInstanceToAttachTo = GetBestVsInstanceToAttachAsDebugger(vsProcessesOtherThanThisOne);

            //_lowerLevelOpertationsService.AttachVisualStudioToProcess(vsProcessesOtherThanThisOne[vsInstanceChoice], targetProcess);

            _consoleWriter.PrintAttachedSuccess(targetProcess, vsInstanceToAttachTo, portNumberParsed);

            _consoleWriter.PrintApplicationsJobCompleteAndExit();

            return 0;
        }

        private int ParsePortNumber(string portNumber)
        {
            try
            {
                return int.Parse(portNumber);
            }
            catch (Exception)
            {
                _consoleWriter.PrintPortNumberMustBeAnIntegerError();
                throw;
            }
        }

        private Process GetBestVsInstanceToAttachAsDebugger(List<Process> vsProcessesOtherThanThisOne)
        {
            List<string> distinctWindowTitles = vsProcessesOtherThanThisOne.Select(x => x.MainWindowTitle.Replace("(Administrator)", string.Empty).Trim())
                .Distinct()
                .ToList();

            List<string> windowTitles = vsProcessesOtherThanThisOne.Select(x => x.MainWindowTitle).ToList();

            //If there are more than 1 different VS projects open
            if (distinctWindowTitles.Count() > 1)
            {
                for (int i = 0; i < distinctWindowTitles.Count(); i++)
                {
                    _consoleWriter.PrintOtherVsInstanceChoices(distinctWindowTitles, i);
                }

                _consoleWriter.PrintGetVsInstanceChoice();

                try
                {
                    //Minus 1 as we increase count of selection on-screen to make it more human readable
                    
                    int choice = int.Parse(Console.ReadLine()) - 1;

                    Process processChoice = vsProcessesOtherThanThisOne.Where(x => x.MainWindowTitle.Contains(windowTitles[choice]))
                        .FirstOrDefault(x => x.MainWindowTitle.Contains("Administrator")) 
                        //If more than 1 exist and can't prioritise by Admin then get oldest running instance
                        ?? vsProcessesOtherThanThisOne.OrderByDescending(x => x.StartTime)
                        .FirstOrDefault(x => x.MainWindowTitle.Contains(windowTitles[choice]));

                    return vsProcessesOtherThanThisOne[choice];
                }
                catch (Exception ex)
                {
                    _consoleWriter.PrintVsInstanceChoiceError();

                    return null;
                }
            }

            //Prioritise attaching debugger to VS instance running as Admin
            return vsProcessesOtherThanThisOne.FirstOrDefault(x => x.MainWindowTitle.Contains("Administrator")) 
                ?? vsProcessesOtherThanThisOne[0];
        }
        
        private List<Process> GetVSProcessesOtherThanThisOne()
        {
            List<Process> otherVsProcesses = Process.GetProcesses()
                .Where(o => o.ProcessName.Contains("devenv")
                //Don't attach to VS debugging the AttachDebuggerByPort app 
                && !o.MainWindowTitle.Contains("AttachDebuggerByPort")).ToList();            

            if (otherVsProcesses.Count == 0)
                _consoleWriter.PrintNoOtherVSInstancesAreOpenToUseAsDebugger();

            return otherVsProcesses;
        }

        
    }
}
