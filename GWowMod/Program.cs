using GWowMod.Requests;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using Refit;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace GWowMod
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
                .AddSingleton<IWowPathProvider, WowPathProvider>()
                .AddMediatR(typeof(Program).Assembly)
                .AddLogging(x => x.AddConsole());

            var foo = Policy.Handle<IOException>()
                .RetryAsync(3);


            serviceCollection.AddRefitClient<ICurseForgeClient>()
                .ConfigureHttpClient(client =>
                {
                    client.BaseAddress = new Uri("https://addons-ecs.forgesvc.net/api");
                    client.Timeout = TimeSpan.FromSeconds(60); // Overall timeout across all tries
                })
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(timeoutPolicy); // We place the timeoutPolicy inside the retryPolicy, to make it time out each try.

            serviceCollection.AddHttpClient<IRequestHandler<UpdateAddonRequest>, UpdateAddonRequestHander>()
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