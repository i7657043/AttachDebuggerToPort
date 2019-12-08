using System.Collections.Generic;
using System.Diagnostics;


namespace AttachDebuggerByPort.Services
{
    public interface IConsoleWriter
    {
        void PrintPortNumberNotAcceptableError();
        void PrintTargetProcessDetails(Process targetProcess, int portNumber);
        void PrintOtherVsInstanceChoices(List<string> distinctWindowTitles, int distinctWindowTitlesChoice);            
        void PrintVsInstanceChoiceError();
        void PrintNoOtherVSInstancesAreOpenToUseAsDebugger();
        void PrintAttachedSuccess(Process targetProcess, Process vsProcessAttaching, int portNumber, int vsInstanceChoice);
        void PrintProcessIdMustBeAnIntegerError();
        void PrintNoProcessesListeningOnSelectedPortError(int portNum);
        void PrintPortNumberMustBeAnIntegerError();
        void PrintApplicationsJobCompleteAndExit();
    }
}