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
                Name = "Attach Debugger By Port",
                FullName = "Attach Debugger By Port"
            };

            ServiceProvider serviceProvider = new ServiceCollection()
            .AddSingleton<IConsoleWriter, ConsoleWriter>()
            .AddSingleton<ILowerLevelOpertationsService, LowerLevelOpertationsService>()
            .AddSingleton<IApplicationManager, ApplicationManager>()
            .BuildServiceProvider();

            app.HelpOption("-?|-h|--help");
            app.VersionOption("--version", () => env.ApplicationVersion);
            CommandOption portOption = app.Option("-p|--port", "", CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {
                IConsoleWriter consoleWriter = serviceProvider.GetService<IConsoleWriter>();

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