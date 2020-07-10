using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace GWowMod
{
    internal interface IWowPathProvider
    {
        Task SaveInstallPath(string installPath);
        Task<string> GetInstallPath();
    }

    internal class WowPathProvider : IWowPathProvider
    {
        private ILogger<WowPathProvider> _logger;

        public WowPathProvider(ILogger<WowPathProvider> logger)
        {
            _logger = logger;
        }

        public async Task SaveInstallPath(string installPath)
        {
            try
            {
                using (var reader = new StreamReader(System.IO.File.Open(GetSettingsLocation(), FileMode.OpenOrCreate, FileAccess.Read,
                    FileShare.ReadWrite)))
                {
                    var settingsText = await reader.ReadToEndAsync();

                    GWowModSettings gWowModSettings;
                    if (!string.IsNullOrWhiteSpace(settingsText))
                    {
                        gWowModSettings = JsonSerializer.Deserialize<GWowModSettings>(settingsText);
                        gWowModSettings.WowInstallPath = installPath;
                    }
                    else
                    {
                        gWowModSettings = new GWowModSettings { WowInstallPath = installPath };
                    }

                    using (var writer = new StreamWriter(System.IO.File.Open(GetSettingsLocation(), FileMode.Truncate, FileAccess.Write,
                        FileShare.ReadWrite)))
                    {
                        await writer.WriteLineAsync(JsonSerializer.Serialize(gWowModSettings));
                    }
                }
            }
            catch (IOException e)
            {
                _logger.LogError(e.ToString());
            }
        }

        public async Task<string> GetInstallPath()
        {
            using (var reader = new StreamReader(System.IO.File.Open(GetSettingsLocation(), FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite)))
            {
                var settingsContents = await reader.ReadToEndAsync();
                if (string.IsNullOrWhiteSpace(settingsContents))
                {
                    return string.Empty;
                }
                
                var gWowModSettings = JsonSerializer.Deserialize<GWowModSettings>(settingsContents);
                return GetNormalizedPath(gWowModSettings.WowInstallPath);
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
            var appdataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var directory = Directory.CreateDirectory(Path.Combine(appdataFolder, "GWowMod"));

            return Path.Combine(directory.FullName, "GWowModSettings.json");
        }
        
        private class GWowModSettings
        {
            public string WowInstallPath { get; set; }
        }
    }
}