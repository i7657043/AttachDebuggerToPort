﻿using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace AttachDebuggerByPort.Services
{
    public class ConsoleWriter : IConsoleWriter
    {
        private ConsoleColor defaultColour = Console.ForegroundColor;

        public void PrintPortNumberNotAcceptableError()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("The Port number flag (-p/--port) must exist and be an Integer. Please try again or see the help page (--h/--help) for details. Goodbye.");
            Console.ForegroundColor = defaultColour;
        }

        public void PrintTargetProcessDetails(Process targetProcess, int portNumber)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nTarget Process {targetProcess.ProcessName} (Id: {targetProcess.Id}) found running on Port {portNumber}\n");
            Console.ForegroundColor = defaultColour;
        }

        public void PrintOtherVsInstanceChoices(List<string> distinctWindowTitles, int distinctWindowTitlesChoice)
        {
            Console.WriteLine($"Please enter [{distinctWindowTitlesChoice + 1}] to attach the VS instance titled the following to the target process:");
            defaultColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(distinctWindowTitles[distinctWindowTitlesChoice] + "\n");
            Console.ForegroundColor = defaultColour;
        }

        public void PrintVsInstanceChoiceError()
        {
            defaultColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("VS Instance choice must be an Integer.\nPlease try again. Goodbye.");
            Console.ForegroundColor = defaultColour;
        }
        public void PrintNoOtherVSInstancesAreOpenToUseAsDebugger()
        {
            defaultColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("No free VS Instance are open.\nPlease start some and try again. Goodbye.");
            Console.ForegroundColor = defaultColour;
        }

        public void PrintAttachedSuccess(Process targetProcess, Process vsProcessAttaching, int portNumber, int vsInstanceChoice)
        {
            defaultColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nAttached VS Instance (PID: {vsProcessAttaching.Id}, " +
                $"Process Name: {vsProcessAttaching.MainWindowTitle})\n");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"TO\n");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Application Instance (PID: {targetProcess.Id}, " +
                $"Process Name: {targetProcess.MainWindowTitle}-{targetProcess.ProcessName} on Port {portNumber})");
            Console.ForegroundColor = defaultColour;
        }
        public void PrintProcessIdMustBeAnIntegerError()
        {
            ConsoleColor defaultColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nProcess Id must be an Integer.\nPlease try again. Goodbye.");
            Console.ForegroundColor = defaultColour;
        }

        public void PrintNoProcessesListeningOnSelectedPortError(int portNum)
        {
            ConsoleColor defaultColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nNo processes are listening on port {portNum}.\nPlease try again. Goodbye.");
            Console.ForegroundColor = defaultColour;
        }

        public void PrintPortNumberMustBeAnIntegerError()
        {
            ConsoleColor defaultColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Port number must be an Integer. Please try again. Goodbye.");
            Console.ForegroundColor = defaultColour;
        }

        public void PrintApplicationsJobCompleteAndExit()
        {
            Console.WriteLine("\n\nPress any key to Exit. The debugger WILL NOT detach.\n");
            Console.ReadKey();
        }
    }
}