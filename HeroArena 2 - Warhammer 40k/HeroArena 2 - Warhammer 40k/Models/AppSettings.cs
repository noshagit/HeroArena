namespace HeroArena.Models
{
    public class AppSettings
    {
        private static readonly AppSettings _instance = new AppSettings();

        public static AppSettings Instance => _instance;

        public string ConnectionString { get; set; } = "Server=NOSHA\\SQLEXPRESS;Database=ExerciceHero;Trusted_Connection=True;TrustServerCertificate=True;";
        public Player? CurrentPlayer { get; set; }
        public Hero? SelectedHero { get; set; }

        private AppSettings()
        {
        }
    }
}
