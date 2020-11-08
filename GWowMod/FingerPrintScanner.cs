using GWowMod.Curse.Hashing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GWowMod
{
    public interface IFingerPrintScanner
    {
        IEnumerable<long> GetFingerPrints(Game game);
    }

    internal class FingerPrintScanner : IFingerPrintScanner
    {
        public IEnumerable<long> GetFingerPrints(Game game)
        {
            CategorySection section = game.categorySections.FirstOrDefault();

            if (section == null)
            {
                throw new InvalidOperationException("Invalid number of sections found");
            }

            var fingerPrints = new List<long>();

            foreach (var p in section.Directory.GetDirectories())
            {
                var matchingFiles = GetMatchingFiles(game, section, p);
                // FileSystemInfo[] infos = p.GetFileSystemInfos();
                //
                // var foo = MurmurHash2.ComputeHash(Encoding.ASCII.GetBytes(string.Join(",", infos.Select(n => n.LastWriteTimeUtc.Ticks.ToString()).ToArray())), false).ToString();

                if (matchingFiles == null || matchingFiles.Count == 0)
                {
                    continue;
                }

                matchingFiles.Sort();
                List<long> longList = new List<long>();
                foreach (string path in matchingFiles)
                {
                    long normalizedFileHash;
                    normalizedFileHash = MurmurHash2.ComputeNormalizedFileHash(path);

                    longList.Add(normalizedFileHash);
                }

                longList.Sort();

                string empty = string.Empty;
                foreach (long num in longList)
                {
                    empty += num.ToString();
                }

                byte[] bytes = Encoding.ASCII.GetBytes(empty);
                var fingerPrint = (long)MurmurHash2.ComputeHash(bytes);

                fingerPrints.Add(fingerPrint);
            }

            return fingerPrints;
        }

        private List<string> GetMatchingFiles(Game game, CategorySection section,
            DirectoryInfo pFolder)
        {
            try
            {
                List<string> matchingFileList = new List<string>();
                List<FileInfo> fileInfoList = new List<FileInfo>();
                string str = section.Directory.FullName;
                foreach (FileInfo file in pFolder.GetFiles("*.*", SearchOption.AllDirectories))
                {
                    if (section.initialInclusionPattern == "." && section.extraIncludePattern == ".")
                    {
                        matchingFileList.Add(file.FullName.ToLowerInvariant());
                    }
                    else
                    {
                        string input = file.FullName.ToLower().Replace(str.ToLower(), "");
                        if (section.InitialInclusionRegex.Match(input).Success)
                        {
                            fileInfoList.Add(file);
                        }

                        if (section.ExtraIncludeRegex != null && section.ExtraIncludeRegex.Match(input).Success)
                        {
                            matchingFileList.Add(file.FullName.ToLowerInvariant());
                        }
                    }
                }

                foreach (FileInfo pIncludeFile in fileInfoList)
                {
                    ProcessIncludeFile(game, section, matchingFileList, pIncludeFile);
                }

                return matchingFileList;
            }
            catch (Exception ex)
            {
                // Folder.Logger.Error(ex, "Error matching files", (object) null);
                return null;
            }
        }

        private void ProcessIncludeFile(Game game, CategorySection section,
            List<string> matchingFileList,
            FileInfo pIncludeFile)
        {
            try
            {
                if (!pIncludeFile.Exists || matchingFileList.Contains(pIncludeFile.FullName.ToLowerInvariant()))
                {
                    return;
                }

                if (section.packageType != GameSectionPackageMapPackageType.Ctoc && section.packageType != GameSectionPackageMapPackageType.Cmod2)
                {
                    matchingFileList.Add(pIncludeFile.FullName.ToLowerInvariant());
                }

                if (game.fileParsingRules.Count == 0)
                {
                    return;
                }

                string input = null;
                using (StreamReader streamReader = new StreamReader(pIncludeFile.FullName))
                {
                    input = streamReader.ReadToEnd();
                    streamReader.Close();
                }

                FileParsingRule gameFileParsingRule =
                    game.fileParsingRules.FirstOrDefault(p =>
                        p.fileExtension == pIncludeFile.Extension.ToLowerInvariant());
                if (gameFileParsingRule == null)
                {
                    return;
                }

                if (gameFileParsingRule.CommentStripRegex != null)
                {
                    input = gameFileParsingRule.CommentStripRegex.Replace(input, string.Empty);
                }

                foreach (Match match in gameFileParsingRule.InclusionRegex.Matches(input))
                {
                    string fileName;
                    try
                    {
                        string str = match.Groups[1].Value;
                        // if (FilePathHasInvalidChars(str))
                        // {
                        //   // Folder.Logger.Error("Invalid include File", (object) new
                        //   // {
                        //   //   File = str
                        //   // });
                        //   break;
                        // }
                        fileName = Path.Combine(pIncludeFile.DirectoryName, str);
                    }
                    catch (Exception ex)
                    {
                        // Folder.Logger.Error(ex, "Invalid include File", match.Groups.Count > 1 ? (object) new
                        // {
                        //   File = match.Groups[1].Value
                        // } : (object) null);
                        break;
                    }

                    ProcessIncludeFile(game, section, matchingFileList, new FileInfo(fileName));
                }
            }
            catch (Exception ex)
            {
                // Folder.Logger.Error(ex, (string) null, (object) null);
            }

            // public static bool FilePathHasInvalidChars(string path)
            // {
            //     return !string.IsNullOrEmpty(path) && path.IndexOfAny(System.InvalidP) >= 0;
            // }
        }
    }
}