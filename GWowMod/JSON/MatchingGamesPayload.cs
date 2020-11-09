using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace GWowMod.JSON
{
    public class Dependency
    {
        public int id { get; set; }
        public int addonId { get; set; }
        public int type { get; set; }
        public int fileId { get; set; }
    }

    public class Module
    {
        public string foldername { get; set; }
        public object fingerprint { get; set; }
        public int type { get; set; }
    }

    public class SortableGameVersion
    {
        public string gameVersionPadded { get; set; }
        public string gameVersion { get; set; }
        public DateTime gameVersionReleaseDate { get; set; }
        public string gameVersionName { get; set; }
    }

    public class File
    {
        [JsonProperty("Id")]
        public int Id { get; set; }
        public string displayName { get; set; }

        [JsonProperty("FileName")]
        public string FileName { get; set; }

        [JsonProperty("FileDate")]
        public DateTime FileDate { get; set; }
        public int fileLength { get; set; }

        [JsonProperty("ReleaseType")]
        public ProjectFileReleaseType ReleaseType { get; set; }
        public int fileStatus { get; set; }

        [JsonProperty("DownloadUrl")]
        public string DownloadUrl { get; set; }

        [JsonProperty("IsAlternate")]
        public bool IsAlternate { get; set; }
        public int alternateFileId { get; set; }
        public List<Dependency> dependencies { get; set; }
        public bool isAvailable { get; set; }
        public List<Module> modules { get; set; }
        public object packageFingerprint { get; set; }
        public List<string> gameVersion { get; set; }
        public List<SortableGameVersion> sortableGameVersion { get; set; }
        public object installMetadata { get; set; }
        public object changelog { get; set; }
        public bool hasInstallScript { get; set; }
        public bool isCompatibleWithClient { get; set; }
        public GameSectionPackageMapPackageType categorySectionPackageType { get; set; }
        public int restrictProjectFileAccess { get; set; }
        public int projectStatus { get; set; }
        public int renderCacheId { get; set; }
        public object fileLegacyMappingId { get; set; }
        public int projectId { get; set; }
        public object parentProjectFileId { get; set; }
        public object parentFileLegacyMappingId { get; set; }
        public object fileTypeId { get; set; }
        public object exposeAsAlternative { get; set; }
        public int packageFingerprintId { get; set; }
        public DateTime gameVersionDateReleased { get; set; }
        public int gameVersionMappingId { get; set; }

        [JsonProperty("gameVersionId")]
        public int GameVersionId { get; set; }
        public int gameId { get; set; }
        public bool isServerPack { get; set; }
        public object serverPackFileId { get; set; }

        [JsonProperty("gameVersionFlavor")]
        public string GameVersionFlavor { get; set; }
    }

    public class Dependency2
    {
        public int id { get; set; }
        public int addonId { get; set; }
        public int type { get; set; }
        public int fileId { get; set; }
    }

    public class Module2
    {
        public string foldername { get; set; }
        public object fingerprint { get; set; }
        public int type { get; set; }
    }

    public class SortableGameVersion2
    {
        public string gameVersionPadded { get; set; }
        public string gameVersion { get; set; }
        public DateTime gameVersionReleaseDate { get; set; }
        public string gameVersionName { get; set; }
    }

    public class LatestFile
    {
        [JsonProperty("Id")]
        public int Id { get; set; }
        public string displayName { get; set; }

        [JsonProperty("FileName")]
        public string FileName { get; set; }

        [JsonProperty("FileDate")]
        public DateTime FileDate { get; set; }
        public int fileLength { get; set; }

        [JsonProperty("ReleaseType")]
        public ProjectFileReleaseType ReleaseType { get; set; }
        public int fileStatus { get; set; }

        [JsonProperty("DownloadUrl")]
        public string DownloadUrl { get; set; }

        [JsonProperty("IsAlternate")]
        public bool IsAlternate { get; set; }
        public int alternateFileId { get; set; }
        public List<Dependency2> dependencies { get; set; }

        [JsonProperty("IsAvailable")]
        public bool IsAvailable { get; set; }
        public List<Module2> modules { get; set; }
        public object packageFingerprint { get; set; }
        public List<string> gameVersion { get; set; }
        public List<SortableGameVersion2> sortableGameVersion { get; set; }
        public object installMetadata { get; set; }
        public object changelog { get; set; }
        public bool hasInstallScript { get; set; }
        public bool isCompatibleWithClient { get; set; }
        public int categorySectionPackageType { get; set; }
        public int restrictProjectFileAccess { get; set; }
        public int projectStatus { get; set; }
        public int renderCacheId { get; set; }
        public object fileLegacyMappingId { get; set; }
        public int projectId { get; set; }
        public int? parentProjectFileId { get; set; }
        public object parentFileLegacyMappingId { get; set; }
        public int? fileTypeId { get; set; }
        public bool? exposeAsAlternative { get; set; }
        public int packageFingerprintId { get; set; }
        public DateTime gameVersionDateReleased { get; set; }
        public int gameVersionMappingId { get; set; }

        [JsonProperty("gameVersionId")]
        public int GameVersionId { get; set; }
        public int gameId { get; set; }
        public bool isServerPack { get; set; }
        public object serverPackFileId { get; set; }

        [JsonProperty("gameVersionFlavor")]
        public string GameVersionFlavor { get; set; }
    }

    public class ExactMatch
    {
        public int Id { get; set; }

        [JsonProperty("file")]
        public File File { get; set; }

        [JsonProperty("LatestFiles")]
        public List<LatestFile> LatestFiles { get; set; }

        private LatestFile _latestFile;

        public LatestFile LatestFile
        {
            get
            {
                return _latestFile ??= LatestFiles
                    .Where(x => x.GameVersionFlavor == File.GameVersionFlavor)
                    .Where(x => x.ReleaseType == File.ReleaseType)
                    .Where(x => x.IsAlternate == File.IsAlternate)
                    .Where(x => x.IsAvailable)
                    .OrderByDescending(x => x.FileDate)
                    .FirstOrDefault();
            }
        }
    }

    public class PartialMatchFingerprints
    {
    }

    public class MatchingGamesPayload
    {
        public bool isCacheBuilt { get; set; }
        public List<ExactMatch> exactMatches { get; set; }
        public List<object> exactFingerprints { get; set; }
        public List<object> partialMatches { get; set; }
        public PartialMatchFingerprints partialMatchFingerprints { get; set; }
        public List<object> installedFingerprints { get; set; }
        public List<object> unmatchedFingerprints { get; set; }
    }

    public enum ProjectFileReleaseType
    {
        Release = 1,
        Beta = 2,
        Alpha = 3
    }
}
//
// public bool IsAddonUpdateAllowed
// {
// get
// {
//     return !this.IsInstalling && !this.IsFuzzyMatch && this.HasFile && this.HasUpdate;
// }
// }

//
// public bool HasUpdate
// {
// get
// {
//     return !this.IsWorkingCopy && this.IsInstalled && (!this.IsFuzzyMatch && !this.PreferenceIsIgnored) && (this.InstalledFile.Id != 0 && this.LatestFile != null && !(this.InstalledFile.FileDate == this.LatestFile.FileDate)) && (this.InstalledFile.FileDate < this.LatestFile.FileDate || this.InstalledFile.IsAlternate != this.LatestFile.IsAlternate && this.LatestFile.IsAlternate == this.PreferenceAlternateFile);
// }
// }