using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace GWowMod.Console
{
    public class Program
    {
        /// <summary>
        /// </summary>
        /// <param name="config">Get Wow Install Path</param>
        /// <param name="installPath">Set Wow Install Path e.g. C:\Program Files (x86)\World of Warcraft\_retail_\Interface\AddOns</param>
        /// <param name="updateAddons">Update all out of date addons</param>
        /// <param name="addons">Return list of installed addons</param>
        /// <param name="updateAddon">Update addon by Id</param>
        /// <returns></returns>
        private static async Task Main(bool config, string installPath, bool updateAddons, bool addons, int? updateAddon)
        {
            var cliOptions = new CliOptions
            {
                Config = config,
                InstallPath = installPath,
                UpdateAddons = updateAddons,
                Addons = addons,
                UpdateAddon = updateAddon
            };

            await Startup(cliOptions);
        }

        private static async Task Startup(CliOptions cliOptions)
        {
            //setup our DI
            var serviceCollection = new ServiceCollection()
                .AddSingleton<IGWowModWorker, GWowModWorker>()
                .AddLogging(x => x.AddConsole());

            serviceCollection.AddGWowMod();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var logger = serviceProvider.GetService<ILoggerFactory>()
                .CreateLogger<Program>();
            logger.LogDebug("Starting application");

            try
            {
                var gWowModWorker = serviceProvider.GetService<IGWowModWorker>();
                await gWowModWorker.Run(cliOptions);
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
            }

            logger.LogDebug("All done!");
        }

        public class CliOptions
        {
            public bool Config { get; set; }
            public string InstallPath { get; set; }
            public bool UpdateAddons { get; set; }
            public bool Addons { get; set; }
            public int? UpdateAddon { get; set; }
        }
    }
}