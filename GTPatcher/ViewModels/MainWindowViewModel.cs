using System.Collections.ObjectModel;
using System.Collections.Generic;
using GTPatcher.Types;
using ReactiveUI;

namespace GTPatcher.ViewModels
{
    public class ListEntry { }

    public class YearHeader : ListEntry
    {
        public int Year { get; set; }
    }

    public class PatchEntry : ListEntry
    {
        public Patch Patch { get; set; } = null!;
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
                }
            }
        }

        private Patch? _selectedPatch;
        public Patch? SelectedPatch
        {
            get => _selectedPatch;
            set => this.RaiseAndSetIfChanged(ref _selectedPatch, value);
        }

        private int _selectedTabIndex;
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
            set => this.RaiseAndSetIfChanged(ref _installationPath, value);
        }
    }
}
