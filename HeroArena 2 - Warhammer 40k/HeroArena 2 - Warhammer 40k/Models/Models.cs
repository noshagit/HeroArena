namespace HeroArena.Models
{
    public class Login
    {
        public int ID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public Player? Player { get; set; }
    }

    public class Player
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? LoginID { get; set; }
        public Login? Login { get; set; }
        public ICollection<PlayerHero> PlayerHeroes { get; set; } = new List<PlayerHero>();
        public int Score { get; set; }
    }

    public class Hero
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Health { get; set; }
        public string? ImageURL { get; set; }
        public ICollection<HeroSpell> HeroSpells { get; set; } = new List<HeroSpell>();
        public ICollection<PlayerHero> PlayerHeroes { get; set; } = new List<PlayerHero>();
    }

    public class Spell
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Damage { get; set; }
        public string? Description { get; set; }
        public ICollection<HeroSpell> HeroSpells { get; set; } = new List<HeroSpell>();
    }

    public class PlayerHero
    {
        public int PlayerID { get; set; }
        public int HeroID { get; set; }
        public Player? Player { get; set; }
        public Hero? Hero { get; set; }
    }

    public class HeroSpell
    {
        public int HeroID { get; set; }
        public int SpellID { get; set; }
        public Hero? Hero { get; set; }
        public Spell? Spell { get; set; }
    }
}
