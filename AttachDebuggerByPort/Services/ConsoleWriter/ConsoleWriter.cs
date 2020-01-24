using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AttachDebuggerByPort.Services
{
    public class ConsoleWriter : IConsoleWriter
    {
        private ConsoleColor defaultColour = Console.ForegroundColor;

        public void PrintHelp()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Please choose a Port number and this app will connect a VS instance to the process listening on it automatically");
            Console.ForegroundColor = defaultColour;
        }

        public void PrintCouldNotAttachError()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nA debugger could not be attached to the target Processes. The application selected as debugger may already be in use.");
            Console.ForegroundColor = defaultColour;
            Console.WriteLine("\nPress any key to Exit. Goodbye.");
            Console.ReadKey();
        }

        public void PrintPortNumberNotAcceptableError()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("The Port number flag (-p/--port) must exist and be an Integer. Please try again or see the help page (--h/--help) for details.");
            Console.ForegroundColor = defaultColour;
            Console.WriteLine("\nPress any key to Exit. Goodbye.");
            Console.ReadKey();
        }

        public void PrintTargetProcessDetails(Process targetProcess, int portNumber)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Target Process {targetProcess.ProcessName} (PID: {targetProcess.Id}) found running on Port {portNumber}");
            Console.ForegroundColor = defaultColour;
        }

        public void PrintGetVsInstanceChoice()
        {
            Console.Write("\nPlease enter your choice: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
        }
        
        public void PrintOtherVsInstanceChoices(List<string> distinctWindowTitles, int distinctWindowTitlesChoice)
        {
            Console.WriteLine();
            ConsoleColor defaultColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Please enter [{distinctWindowTitlesChoice + 1}] to attach the VS instance [{distinctWindowTitles[distinctWindowTitlesChoice]}]");
            Console.ForegroundColor = defaultColour;
        }

        public void PrintVsInstanceChoiceError()
        {
            defaultColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("VS Instance choice must be an Integer.\nPlease try again.");
            Console.ForegroundColor = defaultColour;
            Console.WriteLine("Press any key to Exit. Goodbye.");
            Console.ReadKey();
        }

        public void PrintNoOtherVSInstancesAreOpenToUseAsDebugger()
        {
            defaultColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("No free VS Instance are open.\nPlease start some and try again.");
            Console.ForegroundColor = defaultColour;
            Console.WriteLine("\nPress any key to Exit. Goodbye.");
            Console.ReadKey();
        }

        public void PrintNotEnoughVSInstancesAreOpenToUseAsDebugger(int numberOfPorts, int freeVsInstances)
        {
            defaultColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Not enough free VS Instance are open. You require {numberOfPorts - freeVsInstances} more VS instances.\nPlease start some more and try again.");
            Console.ForegroundColor = defaultColour;
            Console.WriteLine("\nPress any key to Exit. Goodbye.");
            Console.ReadKey();
        }

        public void PrintAttachedSuccess(List<Process> targetProcesses, Process vsProcessAttaching)
        {
            int counter = 1;

            Console.ForegroundColor = defaultColour;
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine($"\n-------------------------------------------------------------------------------------------------");

            foreach (Process targetProcess in targetProcesses)
            {                
                Console.WriteLine($"Attached VS Instance {vsProcessAttaching.MainWindowTitle.Replace(" - Microsoft Visual Studio", string.Empty)} (PID: {vsProcessAttaching.Id})");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\nTO\n");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Application Instance {targetProcess.ProcessName} (PID: {targetProcess.Id})");
                Console.WriteLine($"-------------------------------------------------------------------------------------------------\n");

                counter++;
            }           
            
            Console.ForegroundColor = defaultColour;
        }
        public void PrintProcessIdMustBeAnIntegerError()
        {
            ConsoleColor defaultColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nProcess Id must be an Integer.\nPlease try again.");
            Console.ForegroundColor = defaultColour;
            Console.WriteLine("\nPress any key to Exit. Goodbye.");
            Console.ReadKey();
        }

        public void PrintNoProcessesListeningOnSelectedPortError(int portNum)
        {
            ConsoleColor defaultColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"No processes are listening on port {portNum}.\nPlease try again.");                        
            Console.ForegroundColor = defaultColour;
            Console.WriteLine("\nPress any key to Exit. Goodbye.");
            Console.ReadKey();
        }

        public void PrintPortNumberMustBeAnIntegerError()
        {
            ConsoleColor defaultColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Port number must be an Integer. Please try again.");
            Console.ForegroundColor = defaultColour;
            Console.WriteLine("\nPress any key to Exit. Goodbye.");
            Console.ReadKey();
        }

        public void PrintPortNumbersMustBeAnIntegerError()
        {
            ConsoleColor defaultColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Port numbers must all be Integers. Please try again.");            
            Console.ForegroundColor = defaultColour;
            Console.WriteLine("\nPress any key to Exit. Goodbye.");
            Console.ReadKey();
        }

        public void PrintApplicationsJobCompleteAndExit()
        {
            ConsoleColor defaultColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("SUCCESS!");
            Console.ForegroundColor = defaultColour;
            Console.WriteLine("\nPress any key to exit the application. The debugger WILL NOT detach.\n");
            Console.ReadKey();
        }
    }
}