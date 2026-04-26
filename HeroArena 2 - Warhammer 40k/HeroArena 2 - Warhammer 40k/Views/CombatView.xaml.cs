using HeroArena.ViewModels;
using System.Windows.Controls;

namespace HeroArena.Views
{
    public partial class CombatView : UserControl
    {
        public CombatView()
        {
            InitializeComponent();
            DataContext = new CombatViewModel();
            IsVisibleChanged += (s, e) =>
            {
                if ((bool)e.NewValue)
                {
                    var vm = (CombatViewModel)DataContext;
                    if (vm.PlayerHero == null)
                        vm.InitCombatCommand.Execute(null);
                }
            };
        }
    }
}