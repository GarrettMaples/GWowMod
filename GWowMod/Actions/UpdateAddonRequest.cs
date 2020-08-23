using GWowMod.JSON;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using File = System.IO.File;

namespace GWowMod.Actions
{
    public class UpdateAddonRequest : IRequest
    {
        public UpdateAddonRequest(params ExactMatch[] exactMatches)
        {
            ExactMatches = exactMatches;
        }

        public ExactMatch[] ExactMatches { get; }
    }

    internal class UpdateAddonRequestHander : IRequestHandler<UpdateAddonRequest>
    {
        private readonly ILogger<UpdateAddonRequestHander> _logger;
        private readonly HttpClient _httpClient;
        private readonly IWowPathProvider _wowPathProvider;

        private readonly ProjectFileReleaseType _preferredReleaseType = ProjectFileReleaseType.Release;

        public UpdateAddonRequestHander(ILogger<UpdateAddonRequestHander> logger, HttpClient httpClient, IWowPathProvider wowPathProvider)
        {
            _logger = logger;
            _httpClient = httpClient;
            _wowPathProvider = wowPathProvider;
        }

        public async Task<Unit> Handle(UpdateAddonRequest request, CancellationToken cancellationToken)
        {
            string installPath = await _wowPathProvider.GetInstallPath();

            foreach (var exactMatch in request.ExactMatches)
            {
                _logger.LogInformation($"Updating {exactMatch.file.modules[0].foldername}...");

                LatestFile actualLatesFile = null;

                foreach (var latestFile in exactMatch.latestFiles)
                {
                    if (exactMatch.file.gameVersionFlavor != null && (latestFile.gameVersionFlavor == null
                        || latestFile.gameVersionFlavor != exactMatch.file.gameVersionFlavor))
                    {
                        continue;
                    }

                    if (exactMatch.file.categorySectionPackageType == GameSectionPackageMapPackageType.Folder)
                    {
                        if (latestFile == null || !latestFile.isAvailable || latestFile.isAlternate)
                        {
                            continue;
                        }
                    }
                    else if (latestFile == null || !latestFile.isAvailable)
                    {
                        continue;
                    }

                    if (latestFile.releaseType == _preferredReleaseType && (actualLatesFile == null || latestFile.fileDate > actualLatesFile.fileDate))
                    {
                        actualLatesFile = latestFile;
                    }
                }

                if (actualLatesFile == null)
                {
                    _logger.LogInformation($"Latest file for {exactMatch.file.modules[0].foldername} not found");
                    return Unit.Value;
                }

                if (actualLatesFile.id == exactMatch.id)
                {
                    _logger.LogInformation($"{exactMatch.file.modules[0].foldername} - Version: {exactMatch.file.fileName} is the latest and is already installed.");
                    return Unit.Value;
                }

                var response = (await _httpClient.GetAsync(actualLatesFile.downloadUrl))
                    .EnsureSuccessStatusCode();

                var workingDirectory = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GWowMod"));
                var fileNameAndPath = Path.Combine(workingDirectory.FullName, actualLatesFile.fileName);

                await using (var file = new FileInfo(fileNameAndPath).Create())
                {
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    await file.WriteAsync(bytes, 0, bytes.Length);
                }

                foreach (var module in exactMatch.file.modules)
                {
                    var moduleToDelete = Path.Combine(installPath, module.foldername);

                    _logger.LogInformation($"Deleting module {moduleToDelete}...");

                    Directory.Delete(moduleToDelete, recursive: true);
                }

                ZipFile.ExtractToDirectory(fileNameAndPath, installPath);

                File.Delete(fileNameAndPath);

                _logger.LogInformation($"Finished updating {exactMatch.file.modules[0].foldername} from {exactMatch.file.fileName} to {actualLatesFile.fileName}...");
            }

            return Unit.Value;
        }
    }
}