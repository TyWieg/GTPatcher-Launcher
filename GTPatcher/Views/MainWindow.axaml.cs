using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http;
using System.Diagnostics;
using System.IO;
using GTPatcher_Launcher.Utilities;
using GTPatcher.Types;
using GTPatcher.ViewModels;
using System.Collections.ObjectModel;
using Avalonia.Platform;

namespace GTPatcher.Views
{
    public partial class MainWindow : Window
    {
        private string SettingsPath;
        private Settings Settings;
        private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext!;

        private async void ShowMessageBox(string title, string message)
        {
            var box = MessageBoxManager
                  .GetMessageBoxStandard(title, message,
                      ButtonEnum.Ok);

            await box.ShowAsync();
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();

            var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GTPatcher");
            if (!Directory.Exists(appData))
            {
                Directory.CreateDirectory(appData);
            }
            SettingsPath = Path.Combine(appData, "settings.json");
            Settings = File.Exists(SettingsPath) ? JsonConvert.DeserializeObject<Settings>(File.ReadAllText(SettingsPath))! : new Settings();

            ViewModel.InstallationPath = Settings.Path ?? string.Empty;
            ViewModel.SteamUsername = Settings.Username ?? string.Empty;

            // Subscribe to settings changes
            ViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainWindowViewModel.InstallationPath) ||
                    e.PropertyName == nameof(MainWindowViewModel.SteamUsername))
                {
                    SaveSettings();
                }
            };

            LoadBuilds();
        }

        private void LoadBuilds()
        {
            try
            {
                // Load from bundled asset
                using (var stream = AssetLoader.Open(new Uri("avares://GTPatcher/Assets/steamBuilds.json")))
                using (var reader = new StreamReader(stream))
                {
                    var buildsJson = reader.ReadToEnd();
                    var builds = JsonConvert.DeserializeObject<List<Patch>>(buildsJson);
                    if (builds != null)
                    {
                        var sortedBuilds = builds.OrderByDescending(p => p.Year).ThenByDescending(p => p.ManifestId).ToList();
                        var entries = new List<ListEntry>();
                        int currentYear = -1;

                        foreach (var build in sortedBuilds)
                        {
                            if (build.Year != currentYear)
                            {
                                currentYear = build.Year;
                                entries.Add(new YearHeader { Year = currentYear });
                            }
                            entries.Add(new PatchEntry { Patch = build });
                        }

                        ViewModel.BuildEntries = new ObservableCollection<ListEntry>(entries);
                        ViewModel.SelectedEntry = ViewModel.BuildEntries.FirstOrDefault(e => e is PatchEntry);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessageBox("Error", "Failed to load builds: " + ex.Message);
            }
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                ShowMessageBox("Hello there, fellow penguin!", "I see you're on Linux.\nBefore you go ahead and download a build, running this application from a terminal is REQUIRED to type your Steam password into DepotDownloader\nGood luck, have fun! :3");
            }
        }

        private void SaveSettings()
        {
            Settings.Path = ViewModel.InstallationPath;
            Settings.Username = ViewModel.SteamUsername;
            File.WriteAllText(SettingsPath, JsonConvert.SerializeObject(Settings));
        }

        private async void PlayButton(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ViewModel.SteamUsername))
            {
                ShowMessageBox("Cannot download game", "Put in your Steam account username in the Settings tab.");
                return;
            }

            if (string.IsNullOrEmpty(ViewModel.InstallationPath))
            {
                ShowMessageBox("Cannot download game", "You don't have an installation path set. Set a folder in the Settings.");
                return;
            }

            var selectedBuild = ViewModel.SelectedPatch;
            if (selectedBuild == null)
            {
                ShowMessageBox("Cannot download game", "You need to select a game version first.");
                return;
            }

            var specificBuildPath = Path.Combine(ViewModel.InstallationPath, selectedBuild.PatchShorthand);
            if (!Directory.Exists(specificBuildPath))
            {
                Directory.CreateDirectory(specificBuildPath);
                if (InstallGame(selectedBuild, specificBuildPath) != 0)
                {
                    ShowMessageBox("Eek...", "Something went wrong!\nMost likely your username or password is incorrect.\nTo prevent problems, the incomplete installation of the game will be deleted.");
                    if (Directory.Exists(specificBuildPath)) Directory.Delete(specificBuildPath, true);
                    return;
                }

                if (!string.IsNullOrEmpty(selectedBuild.PatchLink))
                {
                    PatchAssembly(selectedBuild, Path.Combine(specificBuildPath, $"{selectedBuild.GameName}_Data", "Managed"));
                }
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var exePath = Path.Combine(specificBuildPath, $"{selectedBuild.GameName}.exe");
                if (File.Exists(exePath))
                    Process.Start(exePath);
            }
            else
            {
                ShowMessageBox("Manual action required", "Linux builds of the launcher will not auto-start the game due to complexities with Wine/Proton.\nPlease add the build as a non-Steam game in your Steam library, and run it under Proton.");
            }
        }

        private int InstallGame(Patch selectedBuild, string installPath)
        {
            if (selectedBuild.IsSteam && selectedBuild.ManifestId.HasValue)
            {
                return DownloadHelper.DownloadManifest((ulong)selectedBuild.ManifestId.Value, installPath, ViewModel.SteamUsername, selectedBuild.Branch ?? "public");
            }
            else if (!string.IsNullOrEmpty(selectedBuild.GameLink))
            {
                return DownloadHelper.DownloadUrl(installPath, selectedBuild.GameLink);
            }
            return 1;
        }

        private async void PatchAssembly(Patch selectedBuild, string managedPath)
        {
            if (string.IsNullOrEmpty(selectedBuild.PatchLink)) return;

            var dllPath = Path.Combine(managedPath, "Assembly-CSharp.dll");
            var bakPath = Path.Combine(managedPath, "Assembly-CSharp.bak");

            if (!File.Exists(dllPath)) return;

            File.Move(dllPath, bakPath);
            HttpClient client = new HttpClient();

            var xdeltaPath = Path.Combine(managedPath, "patch.xdelta");
            using (var file = File.Create(xdeltaPath))
            {
                var stream = await client.GetStreamAsync(selectedBuild.PatchLink);
                await stream.CopyToAsync(file);
            }

            using (var input = new FileStream(bakPath, FileMode.Open))
            using (var patch = new FileStream(xdeltaPath, FileMode.Open))
            using (var output = new FileStream(dllPath, FileMode.Create))
            {
                using var decoder = new PleOps.XdeltaSharp.Decoder.Decoder(input, patch, output);
                decoder.Run();
            }
        }

        private async void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Select Installation Path"
            };

            var result = await dialog.ShowAsync(this);
            if (result != null)
            {
                ViewModel.InstallationPath = result;
            }
        }
    }
}
