using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using Refit;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace GWowMod
{
    public class Program
    {
        public class CliOptions
        {
            public bool GetInstallPath { get; set; }
            public string InstallPath { get; set; }
            public bool UpdateAddons { get; set; }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="getInstallPath">Get Wow Install Path</param>
        /// <param name="installPath">Set Wow Install Path e.g. C:\Program Files (x86)\World of Warcraft\_retail_\Interface\AddOns</param>
        /// <param name="updateAddons">Update out of date addons</param>
        /// <returns></returns>
        static async Task Main(bool getInstallPath, string installPath, bool updateAddons)
        {
            var cliOptions = new CliOptions
            {
                GetInstallPath = getInstallPath,
                InstallPath = installPath,
                UpdateAddons = updateAddons
            };

            await Startup(cliOptions);
        }

        private static async Task Startup(CliOptions cliOptions)
        {
            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutRejectedException>() // thrown by Polly's TimeoutPolicy if the inner call times out
                .WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(10)
                });

            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(10); // Timeout for an individual try

            //setup our DI
            var serviceCollection = new ServiceCollection()
                .AddLogging(x => x.AddConsole())
                .AddLogging()
                .AddSingleton<IGWowModWorker, GWowModWorker>()
                .AddSingleton<IFingerPrintScanner, FingerPrintScanner>()
                .AddSingleton<IAddonUpdater, AddonUpdater>()
                .AddSingleton<IWowPathProvider, WowPathProvider>()
                .AddLogging(x => x.AddConsole());

            serviceCollection.AddRefitClient<ICurseForgeClient>()
                .ConfigureHttpClient(client =>
                {
                    client.BaseAddress = new Uri("https://addons-ecs.forgesvc.net/api");
                    client.Timeout = TimeSpan.FromSeconds(60); // Overall timeout across all tries
                })
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(timeoutPolicy); // We place the timeoutPolicy inside the retryPolicy, to make it time out each try.
            
            serviceCollection.AddHttpClient<IAddonUpdater, AddonUpdater>()
                .ConfigureHttpClient(client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(60); // Overall timeout across all tries
                })
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(timeoutPolicy); // We place the timeoutPolicy inside the retryPolicy, to make it time out each try.

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
                Console.WriteLine(e);
            }

            logger.LogDebug("All done!");
        }
    }
}