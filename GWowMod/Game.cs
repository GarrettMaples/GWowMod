using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace GWowMod
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class GameFile
    {
        public int id { get; set; }
        public int gameId { get; set; }
        public bool isRequired { get; set; }
        public string fileName { get; set; }
        public int fileType { get; set; }
        public int platformType { get; set; }
    }

    public class GameDetectionHint
    {
        public int id { get; set; }
        public int hintType { get; set; }
        public string hintPath { get; set; }
        public string hintKey { get; set; }
        public int hintOptions { get; set; }
        public int gameId { get; set; }
    }

    public class FileParsingRule
    {
        private Regex _commentStripRegex;
        private Regex _inclusionRegex;

        public string commentStripPattern { get; set; }
        public string fileExtension { get; set; }
        public string inclusionPattern { get; set; }
        public int gameId { get; set; }
        public int id { get; set; }

        [JsonIgnore]
        public Regex CommentStripRegex
        {
            get
            {
                if (commentStripPattern != null && _commentStripRegex == null)
                {
                    _commentStripRegex = new Regex(commentStripPattern);
                }

                return _commentStripRegex;
            }
        }

        [JsonIgnore]
        public Regex InclusionRegex
        {
            get
            {
                if (_inclusionRegex == null)
                {
                    _inclusionRegex = new Regex(inclusionPattern);
                }

                return _inclusionRegex;
            }
        }
    }

    public class CategorySection
    {
        private Regex _mInitialInclusionRegex;
        private Regex _mExtraIncludeRegex;

        public int id { get; set; }
        public int gameId { get; set; }
        public string name { get; set; }
        public GameSectionPackageMapPackageType packageType { get; set; }
        public string path { get; set; }
        public string initialInclusionPattern { get; set; }
        public string extraIncludePattern { get; set; }
        public int gameCategoryId { get; set; }
        public DirectoryInfo Directory { get; set; }
        public string Path { get; set; } = string.Empty;

        [JsonIgnore]
        public Regex InitialInclusionRegex
        {
            get
            {
                if (_mInitialInclusionRegex == null)
                {
                    _mInitialInclusionRegex = new Regex(initialInclusionPattern);
                }

                return _mInitialInclusionRegex;
            }
        }

        [JsonIgnore]
        public Regex ExtraIncludeRegex
        {
            get
            {
                if (extraIncludePattern != null && _mExtraIncludeRegex == null)
                {
                    _mExtraIncludeRegex = new Regex(extraIncludePattern);
                }

                return _mExtraIncludeRegex;
            }
        }
    }

    public enum GameSectionPackageMapPackageType
    {
        Folder = 1,
        Ctoc = 2,
        SingleFile = 3,
        Cmod2 = 4,
        ModPack = 5,
        Mod = 6
    }

    public class Game
    {
        public int id { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
        public DateTime dateModified { get; set; }
        public List<GameFile> gameFiles { get; set; }
        public List<GameDetectionHint> gameDetectionHints { get; set; }
        public List<FileParsingRule> fileParsingRules { get; set; }
        public List<CategorySection> categorySections { get; set; }
        public int maxFreeStorage { get; set; }
        public int maxPremiumStorage { get; set; }
        public int maxFileSize { get; set; }
        public string addonSettingsFolderFilter { get; set; }
        public string addonSettingsStartingFolder { get; set; }
        public string addonSettingsFileFilter { get; set; }
        public string addonSettingsFileRemovalFilter { get; set; }
        public bool supportsAddons { get; set; }
        public bool supportsPartnerAddons { get; set; }
        public int supportedClientConfiguration { get; set; }
        public bool supportsNotifications { get; set; }
        public int profilerAddonId { get; set; }
        public int twitchGameId { get; set; }
        public int clientGameSettingsId { get; set; }
    }
}