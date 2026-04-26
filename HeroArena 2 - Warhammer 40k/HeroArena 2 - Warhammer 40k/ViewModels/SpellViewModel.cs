using System.Collections.ObjectModel;
using System.Windows.Input;
using HeroArena.Data;
using HeroArena.Models;

namespace HeroArena.ViewModels
{
    public class SpellViewModel : BaseViewModel
    {
        private List<Spell> _allSpells = new List<Spell>();
        private Hero? _filterHero;
        private ObservableCollection<Spell> _filteredSpells = new ObservableCollection<Spell>();
        private ObservableCollection<Hero> _heroes = new ObservableCollection<Hero>();
        private Spell? _selectedSpell;
        private bool _isLoading;

        public ObservableCollection<Spell> FilteredSpells
        {
            get => _filteredSpells;
            set => SetProperty(ref _filteredSpells, value);
        }

        public ObservableCollection<Hero> Heroes
        {
            get => _heroes;
            set => SetProperty(ref _heroes, value);
        }

        public Spell? SelectedSpell
        {
            get => _selectedSpell;
            set
            {
                if (SetProperty(ref _selectedSpell, value))
                {
                    OnPropertyChanged(nameof(HasSelectedSpell));
                }
            }
        }

        public Hero? FilterHero
        {
            get => _filterHero;
            set
            {
                if (SetProperty(ref _filterHero, value))
                {
                    ApplyFilter();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool HasSelectedSpell => SelectedSpell != null;

        public ICommand ClearFilterCommand { get; }

        public SpellViewModel()
        {
            ClearFilterCommand = new RelayCommand(ExecuteClearFilter);

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            IsLoading = true;
            try
            {
                var heroesFromDb = await DatabaseService.GetAllHeroesAsync();
                _allSpells = await DatabaseService.GetAllSpellsAsync();

                var heroList = new List<Hero>
                {
                    new Hero { ID = 0, Name = "Tous les héros" }
                };
                heroList.AddRange(heroesFromDb);

                Heroes = new ObservableCollection<Hero>(heroList);

                if (Heroes.Any())
                {
                    FilterHero = Heroes[0];
                }
                else
                {
                    ApplyFilter();
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ApplyFilter()
        {
            if (_allSpells == null) return;

            if (FilterHero == null || FilterHero.ID == 0)
            {
                FilteredSpells = new ObservableCollection<Spell>(_allSpells);
            }
            else
            {
                var filtered = _allSpells.Where(s =>
                    s.HeroSpells != null &&
                    s.HeroSpells.Any(hs => hs.HeroID == FilterHero.ID));

                FilteredSpells = new ObservableCollection<Spell>(filtered);
            }
        }

        private void ExecuteClearFilter(object? parameter)
        {
            if (Heroes != null && Heroes.Count > 0)
            {
                FilterHero = Heroes[0];
            }
        }
    }
}
