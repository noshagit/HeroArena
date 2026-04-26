using HeroArena.ViewModels;
using System.Windows.Controls;

namespace HeroArena.Views
{
    public partial class HeroView : UserControl
    {
        public HeroView()
        {
            InitializeComponent();
            DataContext = new HeroViewModel();
        }
    }
}