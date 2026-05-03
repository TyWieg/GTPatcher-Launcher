using System.Collections.ObjectModel;
using System.Collections.Generic;
using GTPatcher.Types;
using ReactiveUI;
using System.IO;
using System;
using System.Linq;

namespace GTPatcher.ViewModels
{
    public abstract class ListEntry : ReactiveObject { }

    public class YearHeader : ListEntry
    {
        public int Year { get; set; }
    }

    public class PatchEntry : ListEntry
    {
        public Patch Patch { get; set; } = null!;

        private bool _isDownloaded;
        public bool IsDownloaded
        {
            get => _isDownloaded;
            set => this.RaiseAndSetIfChanged(ref _isDownloaded, value);
        }
    }

    public class MainWindowViewModel : ViewModelBase
    {
        private ObservableCollection<ListEntry> _buildEntries = new();
        public ObservableCollection<ListEntry> BuildEntries
        {
            get => _buildEntries;
            set => this.RaiseAndSetIfChanged(ref _buildEntries, value);
        }

        private ListEntry? _selectedEntry;
        public ListEntry? SelectedEntry
        {
            get => _selectedEntry;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedEntry, value);
                if (value is PatchEntry pe)
                {
                    SelectedPatch = pe.Patch;
                    UpdateDownloadedStatus();
                }
            }
        }

        private Patch? _selectedPatch;
        public Patch? SelectedPatch
        {
            get => _selectedPatch;
            set => this.RaiseAndSetIfChanged(ref _selectedPatch, value);
        }

        private int _selectedTabIndex = 0;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => this.RaiseAndSetIfChanged(ref _selectedTabIndex, value);
        }

        private string _steamUsername = string.Empty;
        public string SteamUsername
        {
            get => _steamUsername;
            set => this.RaiseAndSetIfChanged(ref _steamUsername, value);
        }

        private string _installationPath = string.Empty;
        public string InstallationPath
        {
            get => _installationPath;
            set
            {
                this.RaiseAndSetIfChanged(ref _installationPath, value);
                UpdateDownloadedStatus();
            }
        }

        private string _statusText = "Ready";
        public string StatusText
        {
            get => _statusText;
            set => this.RaiseAndSetIfChanged(ref _statusText, value);
        }

        private string _debugInfo = "Initializing...";
        public string DebugInfo
        {
            get => _debugInfo;
            set => this.RaiseAndSetIfChanged(ref _debugInfo, value);
        }

        public void UpdateDownloadedStatus()
        {
            if (string.IsNullOrEmpty(InstallationPath)) return;

            foreach (var entry in BuildEntries)
            {
                if (entry is PatchEntry pe)
                {
                    try {
                        var path = Path.Combine(InstallationPath, pe.Patch.PatchShorthand);
                        pe.IsDownloaded = Directory.Exists(path) && File.Exists(Path.Combine(path, $"{pe.Patch.GameName}.exe"));
                    } catch { }
                }
            }

            if (SelectedEntry is PatchEntry selectedPe)
            {
                StatusText = selectedPe.IsDownloaded ? "Installed" : "Ready to Download";
            }
        }
    }
}
