using System.Collections.ObjectModel;
using GTPatcher.Types;
using ReactiveUI;

namespace GTPatcher.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ObservableCollection<Patch> _groupedBuilds = new();
        public ObservableCollection<Patch> GroupedBuilds
        {
            get => _groupedBuilds;
            set => this.RaiseAndSetIfChanged(ref _groupedBuilds, value);
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
