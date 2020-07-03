using GWowMod.Curse.Hashing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using Refit;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace GWowMod
{
    class Program
    {
        static void Main()
        {
            // var path = @"C:\Program Files (x86)\World of Warcraft\_retail_\Interface\AddOns\ZPerl_PartyPet";
            var path = @"C:\Users\garre\AppData\Roaming";
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                
            }
            
            var normalizedFileHash = MurmurHash2.ComputeNormalizedFileHash(path);
            Console.WriteLine(normalizedFileHash);

            Console.ReadKey();
        }
        
        static async Task Main2(string[] args)
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
                .AddSingleton<CurseForgeApi>();
            
            // var handler = new HttpClientHandler() { UseCookies = true };

            serviceCollection.AddRefitClient<ICurseForgeClient>()
                .ConfigureHttpClient(client =>
                {
                    client.BaseAddress = new Uri("https://www.curseforge.com/");
                    client.Timeout = TimeSpan.FromSeconds(60); // Overall timeout across all tries
                    client.DefaultRequestHeaders.Add("User-Agent",
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.106 Safari/537.36");
                    client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                    client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                    client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
                    client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
                    client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
                    client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "none");
                    client.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");
                    client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                    client.DefaultRequestHeaders.Add("Connection", "keep-alive");
                    client.DefaultRequestHeaders.Add("Host", "www.curseforge.com");
                    client.DefaultRequestHeaders.Add("Purpose", "prefetch");
                })
                .ConfigurePrimaryHttpMessageHandler(x =>
                {
                    var cookieContainer = new CookieContainer();
                    var cookieCollection = new CookieCollection();
                    cookieCollection.Add(new Cookie("__cf_bm", "9ac561edba901ed9a8c98d3149d6386498464ca2-1593119328-1800-AQKrgmwhqfm0K/hfvaJnTuQHUsFK6fAC8MakzXfUDaqjshrUSQTRhBAysxCFR3GFr8Vufn2JliCMCFYhUMJTLPo=", "/", "www.curseforge.com"));
                    cookieCollection.Add(new Cookie("__cfduid", "debf97f64d5da25b6ce384715c56e62f31593119327", "/", "www.curseforge.com"));
                    cookieCollection.Add(new Cookie("AWSALB", "Ym05JBgnKmsLQYh5UfOgSYSu9Eqg75puim3T5UpXKFc6Nug6Xubj2ObXnDha7qXb9pSLk7PUrXyhKmxwBLk7hM+yzZKGYU0MVKbK0nNBOA/sdHQKkEoCeyZc8PCP", "/", "www.curseforge.com"));
                    cookieCollection.Add(new Cookie("AWSALBCORS", "Ym05JBgnKmsLQYh5UfOgSYSu9Eqg75puim3T5UpXKFc6Nug6Xubj2ObXnDha7qXb9pSLk7PUrXyhKmxwBLk7hM+yzZKGYU0MVKbK0nNBOA/sdHQKkEoCeyZc8PCP", "/", "www.curseforge.com"));
                    cookieCollection.Add(new Cookie("Unique_ID_v2", "906e32d889af411099d307d5caec746c", "/", "www.curseforge.com"));
                    cookieContainer.Add(cookieCollection);
                    
                    return new HttpClientHandler { UseCookies = true, CookieContainer = cookieContainer };
                })
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(timeoutPolicy); // We place the timeoutPolicy inside the retryPolicy, to make it time out each try.

            var serviceProvider = serviceCollection.BuildServiceProvider();

            //configure console logging
            // serviceProvider
            //     .GetService<ILoggerFactory>();
            // // .AddConsole(LogLevel.Debug);

            var logger = serviceProvider.GetService<ILoggerFactory>()
                .CreateLogger<Program>();
            logger.LogDebug("Starting application");

            try
            {
                var gWowModWorker = serviceProvider.GetService<IGWowModWorker>();
                await gWowModWorker.Run(args);

                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            logger.LogDebug("All done!");
        }
    }
}