using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AttachDebuggerByPort.Extensions
{
    public static class ProcessExtensions
    {
        public static List<Process> FilterProcesses(this List<Process> otherVsProcesses, string filter)
        {
            if (filter != string.Empty)
                return otherVsProcesses.Where(x => x.MainWindowTitle.ToLower().Contains(filter.ToLower())).ToList();

            return otherVsProcesses;
        }
    }
}
