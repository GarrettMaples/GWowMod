using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace GWowMod
{
    public interface IWowPathProvider
    {
        Task SaveInstallPath(string installPath);
        Task<string> GetInstallPath();
        Task<IEnumerable<string>> GetInstallPaths();
        string InstallPathValue { get; set; }
    }

    internal class WowPathProvider : IWowPathProvider
    {
        private readonly ILogger<WowPathProvider> _logger;

        public string InstallPathValue { get; set; } = string.Empty;

        public WowPathProvider(ILogger<WowPathProvider> logger)
        {
            _logger = logger;
        }

        public async Task SaveInstallPath(string installPath)
        {
            var settingsLocation = GetSettingsLocation();
            
            using (var reader = new StreamReader(File.Open(settingsLocation, FileMode.OpenOrCreate, FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete)))
            {
                var settingsText = await reader.ReadToEndAsync();

                GWowModSettings gWowModSettings;
                if (!string.IsNullOrWhiteSpace(settingsText))
                {
                    try
                    {
                        gWowModSettings = JsonSerializer.Deserialize<GWowModSettings>(settingsText);
                    }
                    catch
                    {
                        File.Delete(settingsLocation);
                        await SaveInstallPath(installPath);
                        return;
                    }
                        
                    gWowModSettings.WowInstallPaths.Add(installPath);
                }
                else
                {
                    gWowModSettings = new GWowModSettings { WowInstallPaths = new[] { installPath } };
                }

                await using (var writer = new StreamWriter(File.Open(GetSettingsLocation(), FileMode.Truncate, FileAccess.Write,
                    FileShare.ReadWrite)))
                {
                    await writer.WriteLineAsync(JsonSerializer.Serialize(gWowModSettings));
                }
            }
        }

        public async Task<string> GetInstallPath()
        {
            return (await GetInstallPaths()).FirstOrDefault();
        }

        public async Task<IEnumerable<string>> GetInstallPaths()
        {
            using (var reader = new StreamReader(File.Open(GetSettingsLocation(), FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite)))
            {
                var settingsContents = await reader.ReadToEndAsync();
                if (string.IsNullOrWhiteSpace(settingsContents))
                {
                    return Enumerable.Empty<string>();
                }

                var gWowModSettings = JsonSerializer.Deserialize<GWowModSettings>(settingsContents);
                return gWowModSettings.WowInstallPaths.Select(x => GetNormalizedPath(x));
            }
        }

        private string GetNormalizedPath(string path, bool checkExistence = true)
        {
            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                path += Path.DirectorySeparatorChar.ToString();
            }

            return path;
        }

        private string GetSettingsLocation()
        {
            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var directory = Directory.CreateDirectory(Path.Combine(appDataFolder, "GWowMod"));

            return Path.Combine(directory.FullName, "GWowModSettings.json");
        }

        private class GWowModSettings
        {
            public IList<string> WowInstallPaths { get; set; }
        }
    }
}