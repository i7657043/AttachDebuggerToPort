using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AttachDebuggerByPort.Services
{
    public interface IConsoleWriter
    {
        void PrintHelp();
        void PrintCouldNotAttachError();
        void PrintPortNumberNotAcceptableError();
        void PrintTargetProcessDetails(Process targetProcess, int portNumber);
        void PrintGetVsInstanceChoice();
        void PrintOtherVsInstanceChoices(List<string> distinctWindowTitles, int distinctWindowTitlesChoice);            
        void PrintVsInstanceChoiceError();
        void PrintNoOtherVSInstancesAreOpenToUseAsDebugger();
        void PrintNotEnoughVSInstancesAreOpenToUseAsDebugger(int numberOfPorts, int freeVsInstances);
        void PrintAttachedSuccess(List<Process> targetProcesses, Process vsProcessAttaching);
        void PrintProcessIdMustBeAnIntegerError();
        void PrintNoProcessesListeningOnSelectedPortError(int portNum);
        void PrintPortNumberMustBeAnIntegerError();
        void PrintApplicationsJobCompleteAndExit();
    }
}