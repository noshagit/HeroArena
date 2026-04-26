using System.Collections.ObjectModel;
using System.Windows.Input;
using HeroArena.Data;
using HeroArena.Models;

namespace HeroArena.ViewModels
{
    public class CombatHero
    {
        public string Name { get; set; } = string.Empty;
        public int MaxHealth { get; set; }
        public int CurrentHealth { get; set; }
        public List<Spell> Spells { get; set; } = new List<Spell>();
        public double HealthPercent => MaxHealth == 0 ? 0 : (double)CurrentHealth / MaxHealth * 100;
        public bool IsAlive => CurrentHealth > 0;
    }

    public class CombatLogEntry
    {
        public string Message { get; set; } = string.Empty;
        public string Color { get; set; } = "#E8E0D0";
    }

    public class CombatViewModel : BaseViewModel
    {
        private CombatHero? _playerHero;
        private CombatHero? _enemyHero;
        private ObservableCollection<CombatLogEntry> _combatLog = new ObservableCollection<CombatLogEntry>();
        private List<Spell> _playerSpells = new List<Spell>();
        private bool _isPlayerTurn;
        private bool _isCombatOver;
        private string _combatStatus = string.Empty;
        private int _playerScore;
        private int _enemyLevel = 1;

        public CombatHero? PlayerHero
        {
            get => _playerHero;
            set
            {
                if (SetProperty(ref _playerHero, value))
                {
                    OnPropertyChanged(nameof(PlayerHealthPercent));
                    OnPropertyChanged(nameof(PlayerHealthText));
                }
            }
        }

        public CombatHero? EnemyHero
        {
            get => _enemyHero;
            set
            {
                if (SetProperty(ref _enemyHero, value))
                {
                    OnPropertyChanged(nameof(EnemyHealthPercent));
                    OnPropertyChanged(nameof(EnemyHealthText));
                }
            }
        }

        public ObservableCollection<CombatLogEntry> CombatLog
        {
            get => _combatLog;
            set => SetProperty(ref _combatLog, value);
        }

        public List<Spell> PlayerSpells
        {
            get => _playerSpells;
            set => SetProperty(ref _playerSpells, value);
        }

        public bool IsPlayerTurn
        {
            get => _isPlayerTurn;
            set
            {
                if (SetProperty(ref _isPlayerTurn, value))
                {
                    ((AsyncRelayCommand)UseSpellCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsCombatOver
        {
            get => _isCombatOver;
            set
            {
                if (SetProperty(ref _isCombatOver, value))
                {
                    ((AsyncRelayCommand)UseSpellCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string CombatStatus
        {
            get => _combatStatus;
            set => SetProperty(ref _combatStatus, value);
        }

        public int PlayerScore
        {
            get => _playerScore;
            set => SetProperty(ref _playerScore, value);
        }

        public bool HasHero => AppSettings.Instance.SelectedHero != null;

        public double PlayerHealthPercent => PlayerHero?.HealthPercent ?? 0;
        public double EnemyHealthPercent => EnemyHero?.HealthPercent ?? 0;

        public string PlayerHealthText => $"{PlayerHero?.CurrentHealth ?? 0} / {PlayerHero?.MaxHealth ?? 0}";
        public string EnemyHealthText => $"{EnemyHero?.CurrentHealth ?? 0} / {EnemyHero?.MaxHealth ?? 0}";

        public ICommand InitCombatCommand { get; }
        public ICommand UseSpellCommand { get; }
        public ICommand NewCombatCommand { get; }

        public CombatViewModel()
        {
            InitCombatCommand = new AsyncRelayCommand(ExecuteInitCombatAsync);
            UseSpellCommand = new AsyncRelayCommand(ExecuteUseSpellAsync, CanUseSpell);
            NewCombatCommand = new AsyncRelayCommand(ExecuteNewCombatAsync);

            PlayerScore = AppSettings.Instance.CurrentPlayer?.Score ?? 0;
        }

        private async Task ExecuteInitCombatAsync(object? parameter)
        {
            var dbHero = AppSettings.Instance.SelectedHero;
            if (dbHero == null)
            {
                OnPropertyChanged(nameof(HasHero));
                return;
            }

            var allHeroes = await DatabaseService.GetAllHeroesAsync();
            var fullDbHero = allHeroes.FirstOrDefault(h => h.ID == dbHero.ID);

            if (fullDbHero == null) return;

            PlayerHero = new CombatHero
            {
                Name = fullDbHero.Name,
                MaxHealth = fullDbHero.Health,
                CurrentHealth = fullDbHero.Health,
                Spells = fullDbHero.HeroSpells.Select(hs => hs.Spell!).ToList()
            };

            PlayerSpells = PlayerHero.Spells;

            var otherHeroes = allHeroes.Where(h => h.ID != fullDbHero.ID).ToList();
            if (otherHeroes.Any())
            {
                var random = new Random();
                var randomEnemy = otherHeroes[random.Next(otherHeroes.Count)];

                double hpBonus = 1.0 + 0.1 * (_enemyLevel - 1);
                int enemyMaxHp = (int)(randomEnemy.Health * hpBonus);

                EnemyHero = new CombatHero
                {
                    Name = $"Lvl {_enemyLevel} {randomEnemy.Name}",
                    MaxHealth = enemyMaxHp,
                    CurrentHealth = enemyMaxHp,
                    Spells = randomEnemy.HeroSpells.Select(hs => hs.Spell!).ToList()
                };
            }

            CombatLog.Clear();
            IsPlayerTurn = true;
            IsCombatOver = false;
            CombatStatus = string.Empty;

            RefreshHealthBindings();
            OnPropertyChanged(nameof(HasHero));
        }

        private bool CanUseSpell(object? parameter)
        {
            return IsPlayerTurn && !IsCombatOver && HasHero;
        }

        private async Task ExecuteUseSpellAsync(object? parameter)
        {
            if (parameter is not Spell spell || EnemyHero == null || PlayerHero == null)
                return;

            var random = new Random();
            int baseDamage = spell.Damage + random.Next(-10, 11);
            if (baseDamage < 0) baseDamage = 0;

            EnemyHero.CurrentHealth -= baseDamage;
            if (EnemyHero.CurrentHealth < 0) EnemyHero.CurrentHealth = 0;

            AddLog($"{PlayerHero.Name} utilise {spell.Name} et inflige {baseDamage} dégâts !", "#8B1A1A");
            RefreshHealthBindings();

            if (!EnemyHero.IsAlive)
            {
                PlayerScore += 100 * _enemyLevel;
                if (AppSettings.Instance.CurrentPlayer != null)
                {
                    await DatabaseService.UpdatePlayerScoreAsync(AppSettings.Instance.CurrentPlayer.ID, PlayerScore);
                }

                _enemyLevel++;
                CombatStatus = "VICTOIRE !";
                IsCombatOver = true;
                IsPlayerTurn = false;
                return;
            }

            IsPlayerTurn = false;
            await Task.Delay(900);

            if (EnemyHero.Spells.Any())
            {
                var enemySpell = EnemyHero.Spells[random.Next(EnemyHero.Spells.Count)];
                double dmgBonus = 1.0 + 0.05 * (_enemyLevel - 1);
                int enemyBaseDmg = enemySpell.Damage + random.Next(-10, 11);
                int finalEnemyDmg = (int)(enemyBaseDmg * dmgBonus);

                if (finalEnemyDmg < 0) finalEnemyDmg = 0;

                PlayerHero.CurrentHealth -= finalEnemyDmg;
                if (PlayerHero.CurrentHealth < 0) PlayerHero.CurrentHealth = 0;

                AddLog($"{EnemyHero.Name} riposte avec {enemySpell.Name} et inflige {finalEnemyDmg} dégâts !", "#E8E0D0");
                RefreshHealthBindings();

                if (!PlayerHero.IsAlive)
                {
                    _enemyLevel = 1;
                    CombatStatus = "DÉFAITE...";
                    IsCombatOver = true;
                    IsPlayerTurn = false;
                    return;
                }
            }

            IsPlayerTurn = true;
        }

        private async Task ExecuteNewCombatAsync(object? parameter)
        {
            await ExecuteInitCombatAsync(null);
        }

        private void AddLog(string message, string color)
        {
            CombatLog.Add(new CombatLogEntry { Message = message, Color = color });
        }

        private void RefreshHealthBindings()
        {
            OnPropertyChanged(nameof(PlayerHealthPercent));
            OnPropertyChanged(nameof(EnemyHealthPercent));
            OnPropertyChanged(nameof(PlayerHealthText));
            OnPropertyChanged(nameof(EnemyHealthText));
        }
    }
}
