using System.Collections.Generic;
using System.Diagnostics;


namespace AttachDebuggerByPort.Services
{
    public interface IConsoleWriter
    {
        void PrintHelp();
        void PrintCouldNotAttachError(Process process);
        void PrintPortNumberNotAcceptableError();
        void PrintTargetProcessDetails(Process targetProcess, int portNumber);
        void PrintGetVsInstanceChoice();
        void PrintOtherVsInstanceChoices(List<string> distinctWindowTitles, int distinctWindowTitlesChoice);            
        void PrintVsInstanceChoiceError();
        void PrintNoOtherVSInstancesAreOpenToUseAsDebugger();
        void PrintAttachedSuccess(Process targetProcess, Process vsProcessAttaching, int portNumber);
        void PrintProcessIdMustBeAnIntegerError();
        void PrintNoProcessesListeningOnSelectedPortError(int portNum);
        void PrintPortNumberMustBeAnIntegerError();
        void PrintApplicationsJobCompleteAndExit();
    }
}