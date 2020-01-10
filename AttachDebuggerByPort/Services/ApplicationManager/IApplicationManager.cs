using System.Collections.Generic;
using System.Threading.Tasks;

namespace AttachDebuggerByPort.Services
{
    public interface IApplicationManager
    {
        int AttachDebugger(List<string> portNumber, string filter = "");
    }
}
