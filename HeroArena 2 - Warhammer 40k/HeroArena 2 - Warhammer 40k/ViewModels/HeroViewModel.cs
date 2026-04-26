using System.Collections.ObjectModel;
using System.Windows.Input;
using HeroArena.Data;
using HeroArena.Models;

namespace HeroArena.ViewModels
{
    public class HeroViewModel : BaseViewModel
    {
        private ObservableCollection<Hero> _heroes = new ObservableCollection<Hero>();
        private Hero? _selectedHero;
        private bool _isLoading;
        private string _statusMessage = string.Empty;

        public ObservableCollection<Hero> Heroes
        {
            get => _heroes;
            set => SetProperty(ref _heroes, value);
        }

        public Hero? SelectedHero
        {
            get => _selectedHero;
            set
            {
                if (SetProperty(ref _selectedHero, value))
                {
                    OnPropertyChanged(nameof(SelectedHeroSpells));
                    OnPropertyChanged(nameof(HasSelectedHero));
                    OnPropertyChanged(nameof(IsHeroSelected));
                    StatusMessage = string.Empty;
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public IEnumerable<Spell> SelectedHeroSpells =>
            SelectedHero?.HeroSpells.Select(hs => hs.Spell!) ?? Enumerable.Empty<Spell>();

        public bool HasSelectedHero => SelectedHero != null;

        public bool IsHeroSelected => SelectedHero != null && AppSettings.Instance.SelectedHero?.ID == SelectedHero.ID;

        public ICommand SelectHeroCommand { get; }
        public ICommand RefreshCommand { get; }

        public HeroViewModel()
        {
            SelectHeroCommand = new RelayCommand(ExecuteSelectHero);
            RefreshCommand = new AsyncRelayCommand(ExecuteRefreshAsync);

            RefreshCommand.Execute(null);
        }

        private void ExecuteSelectHero(object? parameter)
        {
            if (SelectedHero != null)
            {
                AppSettings.Instance.SelectedHero = SelectedHero;
                StatusMessage = $"{SelectedHero.Name} prêt au combat !";
                OnPropertyChanged(nameof(IsHeroSelected));
            }
        }

        private async Task ExecuteRefreshAsync(object? parameter)
        {
            IsLoading = true;
            try
            {
                var heroesList = await DatabaseService.GetAllHeroesAsync();
                Heroes = new ObservableCollection<Hero>(heroesList);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
