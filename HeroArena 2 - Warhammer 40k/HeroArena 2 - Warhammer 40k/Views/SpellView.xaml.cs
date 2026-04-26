using HeroArena.ViewModels;
using System.Windows.Controls;

namespace HeroArena.Views
{
    public partial class SpellView : UserControl
    {
        public SpellView()
        {
            InitializeComponent();
            DataContext = new SpellViewModel();
        }
    }
}