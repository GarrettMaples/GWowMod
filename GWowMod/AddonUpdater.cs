using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace GWowMod
{
    internal interface IAddonUpdater
    {
        Task UpdateAddons();
    }

    internal class AddonUpdater : IAddonUpdater
    {
        private readonly IFingerPrintScanner _fingerPrintScanner;
        private readonly ICurseForgeClient _curseForgeClient;
        private readonly HttpClient _httpClient;
        private readonly ILogger<AddonUpdater> _logger;
        private IWowPathProvider _wowPathProvider;

        public AddonUpdater(IFingerPrintScanner fingerPrintScanner, ICurseForgeClient curseForgeClient, HttpClient httpClient, ILogger<AddonUpdater> logger, IWowPathProvider wowPathProvider)
        {
            _fingerPrintScanner = fingerPrintScanner;
            _curseForgeClient = curseForgeClient;
            _httpClient = httpClient;
            _logger = logger;
            _wowPathProvider = wowPathProvider;
        }

        public async Task UpdateAddons()
        {
            var installPath = await _wowPathProvider.GetInstallPath();
            
            if (string.IsNullOrWhiteSpace(installPath))
            {
                throw new InvalidOperationException($"Unable to update addons - WoW Install Path not set");
            }
            
            _logger.LogInformation($"Updating addons in {installPath}...");
            
            var game = JsonSerializer.Deserialize<Game>(await System.IO.File.ReadAllTextAsync(@"C:\Users\garre\source\repos\GWowMod\Configs\Wow Game Instance.json"));
            CategorySection section = game.categorySections.FirstOrDefault();
            section.SetDirectory(installPath);
            
            if (section == null)
            {
                throw new InvalidOperationException("Invalid number of sections found");
            }
            
            var fingerPrints = _fingerPrintScanner.GetFingerPrints(game)

                //.Where(x => x == 1279470483 || x == 1043189886 || x == 340249534 || x == 2976316682 || x == 4259637224)
                .ToList();
            
            _logger.LogInformation($"FingerPrint count: {fingerPrints.Count}");

            if (!fingerPrints.Any())
            {
                return;
            }

            var matchingGamesPayload = await _curseForgeClient.GetMatchingGames(fingerPrints.ToArray());

            foreach (var exactMatch in matchingGamesPayload.exactMatches)
            {
                _logger.LogInformation($"Updating {exactMatch.file.fileName}...");
                
                LatestFile actualLatesFile = null;
                
                foreach (var latestFile in exactMatch.latestFiles)
                {
                    if (exactMatch.file.gameVersionFlavor != null && (latestFile.gameVersionFlavor == null || latestFile.gameVersionFlavor != exactMatch.file.gameVersionFlavor))
                    {
                        continue;
                    }
                    
                    if (exactMatch.file.categorySectionPackageType == GameSectionPackageMapPackageType.Folder)
                    {
                        if (latestFile == null || !latestFile.isAvailable || (latestFile.isAlternate))
                            continue;
                    }
                    else if (latestFile == null || !latestFile.isAvailable)
                    {
                        continue;
                    }

                    if (actualLatesFile == null || latestFile.fileDate > actualLatesFile.fileDate)
                    {
                        actualLatesFile = latestFile;
                    }
                }

                if (actualLatesFile == null)
                {
                    _logger.LogInformation($"Latest file for {exactMatch.file.fileName} not found");
                    continue;
                }

                if (actualLatesFile.id == exactMatch.id)
                {
                    _logger.LogInformation($"{actualLatesFile.fileName} - File Id: {actualLatesFile.id} is the latest and is already installed.");
                    continue;
                }

                var response = (await _httpClient.GetAsync(actualLatesFile.downloadUrl))
                    .EnsureSuccessStatusCode();

                var workingDirectory = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GWowMod"));
                var fileNameAndPath = Path.Combine(workingDirectory.FullName, actualLatesFile.fileName);

                await using (var file = new FileInfo(fileNameAndPath).Create())
                {
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    await file.WriteAsync(bytes, 0, bytes.Length );
                }

                foreach (var module in exactMatch.file.modules)
                {
                    var moduleToDelete = Path.Combine(installPath, module.foldername);
                    
                    _logger.LogInformation($"Deleting module {moduleToDelete}...");
                    
                    Directory.Delete(moduleToDelete, recursive: true);
                }
                
                ZipFile.ExtractToDirectory(fileNameAndPath, installPath);
                
                System.IO.File.Delete(fileNameAndPath);
                
                _logger.LogInformation($"Finished updating from {exactMatch.file.fileName} to {actualLatesFile.fileName}...");
            }
        }
    }
}