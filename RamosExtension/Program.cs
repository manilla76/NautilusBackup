using System;
using Topshelf;

namespace RamosExtension
{
    class Program
    {
        static void Main(string[] args)
        {
            var exitCode = HostFactory.Run(x =>
            {
                x.Service<RamosExtension>(s =>
                {
                    s.ConstructUsing(ext => new RamosExtension());
                    s.WhenStarted(ext => ext.Start());
                    s.WhenStopped(ext => ext.Stop());
                });
                x.UseLog4Net("./log4net.config", true);

                x.RunAsLocalSystem();
                x.SetServiceName("ThermoRamosExtension");
                x.SetDisplayName("Thermo Ramos Extension Service");
                x.SetDescription("Used by Thermo OmniView to allow for remote target changes");
                x.DependsOn("ThermoScientificOmniViewService");
            });

            int exitCodeValue = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());
            Environment.ExitCode = exitCodeValue;
        }
    }
}
