using EnvDTE;
using System.Collections.Generic;
using Process = System.Diagnostics.Process;

namespace AttachDebuggerByPort.Services
{
    public interface ILowerLevelOpertationsService
    {
        bool AttachVisualStudioToProcess(Process debuggerProcess, List<Process> targetProcesses);
        bool TryGetVsInstance(int processId, out _DTE instance);
        int GetProcessIdByPortNumber(int portNum);
    }
}
