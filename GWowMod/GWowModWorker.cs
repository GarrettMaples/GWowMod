using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace GWowMod
{
    public interface IGWowModWorker
    {
        Task Run(string[] args);
    }

    public class GWowModWorker : IGWowModWorker
    {
        private readonly CurseForgeApi _curseForgeApi;

        public GWowModWorker(CurseForgeApi curseForgeApi)
        {
            _curseForgeApi = curseForgeApi;
        }

        public async Task Run(string[] args)
        {
            await Task.Run(() => CommandLine.Parser.Default.ParseArguments<Options>(args)
                .WithParsed(async x => await RunOptions(x))
                .WithNotParsed(HandleParseError));
        }

        private async Task RunOptions(Options opts)
        {
            if (!string.IsNullOrWhiteSpace(opts.WowAddonDirectory))
            {
                try
                {
                    var appdataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                    var directory = new DirectoryInfo(Path.Combine(appdataFolder, "GWowMod"));
                    directory.Create();

                    using (var reader = new StreamReader(File.Open(Path.Combine(directory.FullName, "GWowModSettings.json"), FileMode.OpenOrCreate, FileAccess.Read,
                        FileShare.ReadWrite)))
                    {
                        var settingsText = reader.ReadToEnd();

                        GWowModSettings gWowModSettings;
                        if (!string.IsNullOrWhiteSpace(settingsText))
                        {
                            gWowModSettings = JsonSerializer.Deserialize<GWowModSettings>(settingsText);
                            gWowModSettings.WowAddonDirectory = opts.WowAddonDirectory;
                        }
                        else
                        {
                            gWowModSettings = new GWowModSettings { WowAddonDirectory = opts.WowAddonDirectory };
                        }

                        using (var writer = new StreamWriter(File.Open(Path.Combine(appdataFolder, "GWowModSettings.json"), FileMode.Truncate, FileAccess.Write,
                            FileShare.ReadWrite)))
                        {
                            writer.WriteLine(JsonSerializer.Serialize(gWowModSettings));
                        }
                    }
                }
                catch (IOException e)
                {
                    // Inform the user that an error occurred.
                    Console.WriteLine(e.ToString());
                }
            }

            if (opts.GetWowAddonDirectoryRead)
            {
                var appdataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                using (var reader = new StreamReader(File.Open(Path.Combine(appdataFolder, "GWowModSettings.json"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    var gWowModSettings = JsonSerializer.Deserialize<GWowModSettings>(reader.ReadToEnd());
                    Console.WriteLine(gWowModSettings.WowAddonDirectory);
                }
            }

            if (opts.GetAddons)
            {
                var foo = await _curseForgeApi.GetAddons();
            }
        }

        private void HandleParseError(IEnumerable<Error> errs)
        {
            foreach (var err in errs)
            {
                Console.WriteLine(err);
            }
        }

        private class Options
        {
            [Option("wow-addon-directory", HelpText = "Set Wow Addon Directory")]
            public string WowAddonDirectory { get; set; }

            [Option("get-wow-addon-directory", HelpText = "Get Wow Addon Directory", Default = false)]
            public bool GetWowAddonDirectoryRead { get; set; }


            [Option("get-addons", HelpText = "Get Wow Addons", Default = false)]
            public bool GetAddons { get; set; }

            //[Option('r', "read", Required = true, HelpText = "Input files to be processed.")]
            //public IEnumerable<string> InputFiles { get; set; }

            //// Omitting long name, defaults to name of property, ie "--verbose"
            //[Option(
            //  Default = false,
            //  HelpText = "Prints all messages to standard output.")]
            //public bool Verbose { get; set; }

            //[Option("stdin",
            //  Default = false,
            //  HelpText = "Read from stdin")]
            //public bool stdin { get; set; }

            //[Value(0, MetaName = "offset", HelpText = "File offset.")]
            //public long? Offset { get; set; }
        }

        private class GWowModSettings
        {
            public string WowAddonDirectory { get; set; }
        }
    }
}