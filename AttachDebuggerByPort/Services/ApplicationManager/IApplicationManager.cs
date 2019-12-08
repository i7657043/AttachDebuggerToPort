namespace AttachDebuggerByPort.Services
{
    public interface IApplicationManager
    {
        int AttachDebugger(string portNumber, string filter = "");
    }
}
