using AttachDebuggerByPort.Services;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;


namespace AttachDebuggerByPort
{
    class Program
    {       
        static void Main(string[] args)
        {
            ApplicationEnvironment env = PlatformServices.Default.Application;

            CommandLineApplication app = new CommandLineApplication()
            {
                Name = "DebugByPort",
                FullName = "Attach Debugger By Port"
            };

            ServiceProvider serviceProvider = new ServiceCollection()
            .AddSingleton<IConsoleWriter, ConsoleWriter>()
            .AddSingleton<ILowerLevelOpertationsService, LowerLevelOpertationsService>()
            .AddSingleton<IApplicationManager, ApplicationManager>()
            .BuildServiceProvider();

            CommandOption helpOption = app.HelpOption("-?|-h|--help");
            app.VersionOption("--version", () => env.ApplicationVersion);
            CommandOption portOption = app.Option("-p|--port", "", CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {
                IConsoleWriter consoleWriter = serviceProvider.GetService<IConsoleWriter>();

                if (helpOption.HasValue())
                {
                    consoleWriter.PrintHelp();
                    return -0;
                }

                if (!portOption.HasValue())
                {
                    consoleWriter.PrintPortNumberNotAcceptableError();
                    return -1;
                }

                IApplicationManager applicationManager = serviceProvider.GetService<IApplicationManager>();

                return applicationManager.AttachDebugger(portOption.Value());
            });

            int exitCode = app.Execute(args);
        }        
    }
}