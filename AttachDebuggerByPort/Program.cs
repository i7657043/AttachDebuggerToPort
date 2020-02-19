using AttachDebuggerByPort.Services;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using System.Linq;

namespace AttachDebuggerByPort
{
    class Program
    {       
        static void Main(string[] args)
        {
            ApplicationEnvironment env = PlatformServices.Default.Application;

            CommandLineApplication app = new CommandLineApplication()
            {
                Name = "AttachDebuggerByPort",
                FullName = "Attach Debugger By Port"
            };

            app.ShowHelp();

            ServiceProvider serviceProvider = new ServiceCollection()
            .AddSingleton<IConsoleWriter, ConsoleWriter>()
            .AddSingleton<ILowerLevelOpertationsService, LowerLevelOpertationsService>()
            .AddSingleton<IApplicationManager, ApplicationManager>()
            .BuildServiceProvider();

            CommandOption helpOption = app.HelpOption("-?|-h|--help");
            app.VersionOption("--version", () => env.ApplicationVersion);

            CommandOption portOption = app.Option("-p|--port", "Port", CommandOptionType.SingleValue);
            CommandOption filterOption = app.Option("-f|--filter", "VS instance filter", CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {
                IConsoleWriter consoleWriter = serviceProvider.GetService<IConsoleWriter>();

                IApplicationManager applicationManager = serviceProvider.GetService<IApplicationManager>();

                if (!portOption.HasValue())
                {
                    consoleWriter.PrintPortNumberNotAcceptableError();
                    return -1;
                }
                
                return applicationManager.AttachDebugger(
                    portOption.Value().Split(",").ToList(), 
                    filterOption.Value() ?? string.Empty);
            });

            int exitCode = app.Execute(args);
        }        
    }
}