using EnvDTE;
using Process = System.Diagnostics.Process;

namespace AttachDebuggerByPort.Services
{
    public interface ILowerLevelOpertationsService
    {
        bool AttachVisualStudioToProcess(Process visualStudioProcess, Process applicationProcess);
        bool TryGetVsInstance(int processId, out _DTE instance);
        int GetProcessIdByPortNumber(int portNum);
    }
}
