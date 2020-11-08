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

namespace GWowMod.Requests
{
    public class UpdateAddonRequest : IRequest
    {
        public UpdateAddonRequest(params ExactMatch[] exactMatches)
        {
            ExactMatches = exactMatches;
        }

        public ExactMatch[] ExactMatches { get; }
    }

    internal class UpdateAddonRequestHandler : IRequestHandler<UpdateAddonRequest>
    {
        private readonly ILogger<UpdateAddonRequestHandler> _logger;
        private readonly HttpClient _httpClient;
        private readonly IWowPathProvider _wowPathProvider;

        private readonly ProjectFileReleaseType _preferredReleaseType = ProjectFileReleaseType.Release;

        public UpdateAddonRequestHandler(ILogger<UpdateAddonRequestHandler> logger, HttpClient httpClient, IWowPathProvider wowPathProvider)
        {
            _logger = logger;
            _httpClient = httpClient;
            _wowPathProvider = wowPathProvider;
        }

        public async Task<Unit> Handle(UpdateAddonRequest request, CancellationToken cancellationToken)
        {
            var installPath = await _wowPathProvider.GetInstallPath();

            foreach (var exactMatch in request.ExactMatches)
            {
                _logger.LogInformation($"Updating {exactMatch.File.modules[0].foldername}...");

                //LatestFile actualLatestFile = null;

                //foreach (var latestFile in exactMatch.LatestFiles)
                //{
                //    if (exactMatch.File.gameVersionFlavor != null && (latestFile.gameVersionFlavor == null
                //        || latestFile.gameVersionFlavor != exactMatch.File.gameVersionFlavor))
                //    {
                //        continue;
                //    }

                //    if (exactMatch.File.categorySectionPackageType == GameSectionPackageMapPackageType.Folder)
                //    {
                //        if (latestFile == null || !latestFile.IsAvailable || latestFile.IsAlternate)
                //        {
                //            continue;
                //        }
                //    }
                //    else if (latestFile == null || !latestFile.IsAvailable)
                //    {
                //        continue;
                //    }

                //    if (latestFile.ReleaseType == _preferredReleaseType && (actualLatestFile == null || latestFile.FileDate > actualLatestFile.FileDate))
                //    {
                //        actualLatestFile = latestFile;
                //    }
                //}

                if (exactMatch.LatestFile == null)
                {
                    _logger.LogInformation($"Latest File for {exactMatch.File.modules[0].foldername} not found");
                    return Unit.Value;
                }

                if (exactMatch.LatestFile.Id == exactMatch.Id)
                {
                    _logger.LogInformation($"{exactMatch.File.modules[0].foldername} - Version: {exactMatch.File.FileName} is the latest and is already installed.");
                    return Unit.Value;
                }

                var response = (await _httpClient.GetAsync(exactMatch.LatestFile.DownloadUrl, cancellationToken))
                    .EnsureSuccessStatusCode();

                var workingDirectory = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GWowMod"));
                var fileNameAndPath = Path.Combine(workingDirectory.FullName, exactMatch.LatestFile.FileName);

                await using (var file = new FileInfo(fileNameAndPath).Create())
                {
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    await file.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
                }

                foreach (var module in exactMatch.File.modules)
                {
                    var moduleToDelete = Path.Combine(installPath, module.foldername);

                    _logger.LogInformation($"Deleting module {moduleToDelete}...");

                    Directory.Delete(moduleToDelete, recursive: true);
                }

                ZipFile.ExtractToDirectory(fileNameAndPath, installPath);

                File.Delete(fileNameAndPath);

                _logger.LogInformation($"Finished updating {exactMatch.File.modules[0].foldername} from {exactMatch.File.FileName} to {exactMatch.LatestFile.FileName}...");
            }

            return Unit.Value;
        }
    }
}