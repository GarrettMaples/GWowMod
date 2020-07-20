using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GWowMod.Actions
{
    public class UpdateAddonRequest : IRequest
    {
        public UpdateAddonRequest(ExactMatch exactMatch)
        {
            ExactMatch = exactMatch;
        }

        public ExactMatch ExactMatch { get; }
    }

    internal class UpdateAddonRequestHander : IRequestHandler<UpdateAddonRequest>
    {
        private readonly ILogger<UpdateAddonRequestHander> _logger;
        private readonly HttpClient _httpClient;
        private readonly IWowPathProvider _wowPathProvider;

        public UpdateAddonRequestHander(ILogger<UpdateAddonRequestHander> logger, HttpClient httpClient, IWowPathProvider wowPathProvider)
        {
            _logger = logger;
            _httpClient = httpClient;
            _wowPathProvider = wowPathProvider;
        }

        public async Task<Unit> Handle(UpdateAddonRequest request, CancellationToken cancellationToken)
        {
            string installPath = await _wowPathProvider.GetInstallPath();
            
            _logger.LogInformation($"Updating {request.ExactMatch.file.fileName}...");

            LatestFile actualLatesFile = null;

            foreach (var latestFile in request.ExactMatch.latestFiles)
            {
                if (request.ExactMatch.file.gameVersionFlavor != null && (latestFile.gameVersionFlavor == null
                    || latestFile.gameVersionFlavor != request.ExactMatch.file.gameVersionFlavor))
                {
                    continue;
                }

                if (request.ExactMatch.file.categorySectionPackageType == GameSectionPackageMapPackageType.Folder)
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

                if (actualLatesFile == null || latestFile.fileDate > actualLatesFile.fileDate)
                {
                    actualLatesFile = latestFile;
                }
            }

            if (actualLatesFile == null)
            {
                _logger.LogInformation($"Latest file for {request.ExactMatch.file.fileName} not found");
                return Unit.Value;
            }

            if (actualLatesFile.id == request.ExactMatch.id)
            {
                _logger.LogInformation($"{actualLatesFile.fileName} - File Id: {actualLatesFile.id} is the latest and is already installed.");
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

            foreach (var module in request.ExactMatch.file.modules)
            {
                var moduleToDelete = Path.Combine(installPath, module.foldername);

                _logger.LogInformation($"Deleting module {moduleToDelete}...");

                Directory.Delete(moduleToDelete, recursive: true);
            }

            ZipFile.ExtractToDirectory(fileNameAndPath, installPath);

            System.IO.File.Delete(fileNameAndPath);

            _logger.LogInformation($"Finished updating from {request.ExactMatch.file.fileName} to {actualLatesFile.fileName}...");
            
            return Unit.Value;
        }
    }
}