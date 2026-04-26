using Microsoft.EntityFrameworkCore;
using HeroArena.Models;

namespace HeroArena.Data
{
    public class HeroArenaContext : DbContext
    {
        public DbSet<Login> Logins { get; set; } = null!;
        public DbSet<Player> Players { get; set; } = null!;
        public DbSet<Hero> Heroes { get; set; } = null!;
        public DbSet<Spell> Spells { get; set; } = null!;
        public DbSet<PlayerHero> PlayerHeroes { get; set; } = null!;
        public DbSet<HeroSpell> HeroSpells { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(AppSettings.Instance.ConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Login>().ToTable("Login");

            modelBuilder.Entity<Player>()
                .ToTable("Player")
                .Ignore(p => p.Score);

            modelBuilder.Entity<Hero>().ToTable("Hero");

            modelBuilder.Entity<Spell>().ToTable("Spell");

            modelBuilder.Entity<PlayerHero>()
                .ToTable("PlayerHero")
                .HasKey(ph => new { ph.PlayerID, ph.HeroID });

            modelBuilder.Entity<PlayerHero>()
                .HasOne(ph => ph.Player)
                .WithMany(p => p.PlayerHeroes)
                .HasForeignKey(ph => ph.PlayerID);

            modelBuilder.Entity<PlayerHero>()
                .HasOne(ph => ph.Hero)
                .WithMany(h => h.PlayerHeroes)
                .HasForeignKey(ph => ph.HeroID);

            modelBuilder.Entity<HeroSpell>()
                .ToTable("HeroSpell")
                .HasKey(hs => new { hs.HeroID, hs.SpellID });

            modelBuilder.Entity<HeroSpell>()
                .HasOne(hs => hs.Hero)
                .WithMany(h => h.HeroSpells)
                .HasForeignKey(hs => hs.HeroID);

            modelBuilder.Entity<HeroSpell>()
                .HasOne(hs => hs.Spell)
                .WithMany(s => s.HeroSpells)
                .HasForeignKey(hs => hs.SpellID);

            modelBuilder.Entity<Login>()
                .HasOne(l => l.Player)
                .WithOne(p => p.Login)
                .HasForeignKey<Player>(p => p.LoginID);
        }
    }
}
