using GWowMod.JSON;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
        public UpdateAddonRequest(string installPath, params Match[] exactMatches)
        {
            InstallPath = installPath;
            ExactMatches = exactMatches;
        }

        public Match[] ExactMatches { get; }
        public string InstallPath { get; }
    }

    internal class UpdateAddonRequestHandler : IRequestHandler<UpdateAddonRequest>
    {
        private readonly ILogger<UpdateAddonRequestHandler> _logger;
        private readonly HttpClient _httpClient;

        private readonly ProjectFileReleaseType _preferredReleaseType = ProjectFileReleaseType.Release;

        public UpdateAddonRequestHandler(ILogger<UpdateAddonRequestHandler> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<Unit> Handle(UpdateAddonRequest request, CancellationToken cancellationToken)
        {
            var installPath = request.InstallPath;

            foreach (var exactMatch in request.ExactMatches)
            {
                _logger.LogInformation($"Updating {exactMatch.File.Modules[0].Foldername}...");

                //LatestFile actualLatestFile = null;

                //foreach (var latestFile in exactMatch.LatestFiles)
                //{
                //    if (exactMatch.File.GameVersionFlavor != null && (latestFile.GameVersionFlavor == null
                //        || latestFile.GameVersionFlavor != exactMatch.File.GameVersionFlavor))
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
                    _logger.LogInformation($"Latest File for {exactMatch.File.Modules[0].Foldername} not found");
                    continue;
                }

                if (exactMatch.LatestFile.Id == exactMatch.File.Id)
                {
                    _logger.LogInformation($"{exactMatch.File.Modules[0].Foldername} - Version: {exactMatch.File.FileName} is the latest and is already installed.");
                    continue;
                }

                var response = (await _httpClient.GetAsync(exactMatch.LatestFile.DownloadUrl, cancellationToken))
                    .EnsureSuccessStatusCode();

                var workingDirectory = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GWowMod", exactMatch.Id.ToString()));
                var workingFileNameAndPath = Path.Combine(workingDirectory.FullName, exactMatch.LatestFile.FileName);

                await using (var file = new FileInfo(workingFileNameAndPath).Create())
                {
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    await file.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
                }

                var backupModules = new List<(string backupLocation, string originalLocation)>();

                try
                {
                    foreach (var module in exactMatch.File.Modules)
                    {
                        var moduleDirectory = new DirectoryInfo(Path.Combine(installPath, module.Foldername));

                        if (!moduleDirectory.Exists)
                        {
                            continue;
                        }

                        var moduleToDelete = moduleDirectory.FullName;
                        var moduleBackup = Path.Combine(workingDirectory.FullName, module.Foldername);

                        _logger.LogInformation($"Deleting module {moduleToDelete}...");
                        Directory.Move(moduleToDelete, moduleBackup);
                        backupModules.Add((backupLocation: moduleBackup, originalLocation: moduleToDelete));
                    }

                    ZipFile.ExtractToDirectory(workingFileNameAndPath, installPath);
                }
                catch (Exception exOuter)
                {
                    try
                    {
                        foreach (var (backupLocation, originalLocation) in backupModules)
                        {
                            Directory.Move(backupLocation, originalLocation);
                        }
                    }
                    catch (Exception exInner)
                    {
                        throw new AddonInstallException(exOuter.ToString(),
                            new AddonInstallException(exInner.ToString()));
                    }

                    throw new AddonInstallException(exOuter.ToString());
                }
                finally
                {
                    Directory.Delete(workingDirectory.FullName, recursive: true);
                }

                _logger.LogInformation($"Finished updating {exactMatch.File.Modules[0].Foldername} from {exactMatch.File.FileName} to {exactMatch.LatestFile.FileName}...");
            }

            return Unit.Value;
        }
    }
}