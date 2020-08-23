using System;
using System.IO;

namespace GWowMod
{
    public static class CategorySectionExtension
    {
        public static void SetDirectory(this CategorySection section, string gamePath)
        {
            string platformSafePath = section.GetPlatformSafePath();
            if (platformSafePath.Contains("%"))
            {
                string[] strArray = platformSafePath.Split(Path.DirectorySeparatorChar);
                for (int index = 0; index < strArray.Length; ++index)
                {
                    if (strArray[index].StartsWith("%"))
                    {
                        string variable = strArray[index].Replace("%", string.Empty);
                        strArray[index] = variable == "PERSONAL" || variable == "MYDOCUMENTS"
                            ? Environment.GetFolderPath(Environment.SpecialFolder.Personal)
                            : Environment.GetEnvironmentVariable(variable);
                    }
                }

                string fullPath = Path.GetFullPath(string.Join(Path.DirectorySeparatorChar.ToString(), strArray));
                section.Directory = new DirectoryInfo(fullPath);
            }
            else
            {
                section.Directory = new DirectoryInfo(Path.Combine(gamePath, platformSafePath));
            }
        }

        public static string GetPlatformSafePath(this CategorySection section)
        {
            try
            {
                return section.Path == null ? section.Path : section.Path.Replace('\\', Path.DirectorySeparatorChar);
            }
            catch (Exception ex)
            {
                // Logger<>.Error(ex, (string) null, (object) null);
                return section.Path;
            }
        }
    }
}