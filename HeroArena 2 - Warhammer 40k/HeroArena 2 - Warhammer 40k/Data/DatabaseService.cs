using Microsoft.EntityFrameworkCore;
using HeroArena.Models;

namespace HeroArena.Data
{
    public class DatabaseService
    {
        public static async Task<bool> TestConnectionAsync()
        {
            try
            {
                using var context = new HeroArenaContext();
                return await context.Database.CanConnectAsync();
            }
            catch
            {
                return false;
            }
        }

        public static async Task<Login?> AuthenticateAsync(string username, string password)
        {
            using var context = new HeroArenaContext();
            var login = await context.Logins
                .Include(l => l.Player)
                .FirstOrDefaultAsync(l => l.Username == username);

            if (login != null && BCrypt.Net.BCrypt.Verify(password, login.PasswordHash))
            {
                return login;
            }
            return null;
        }

        public static async Task<Player?> GetPlayerByLoginAsync(int loginId)
        {
            using var context = new HeroArenaContext();
            return await context.Players
                .Include(p => p.Login)
                .FirstOrDefaultAsync(p => p.LoginID == loginId);
        }

        public static async Task<List<Hero>> GetAllHeroesAsync()
        {
            using var context = new HeroArenaContext();
            return await context.Heroes
                .Include(h => h.HeroSpells)
                    .ThenInclude(hs => hs.Spell)
                .ToListAsync();
        }

        public static async Task<List<Spell>> GetAllSpellsAsync()
        {
            using var context = new HeroArenaContext();
            return await context.Spells
                .Include(s => s.HeroSpells)
                    .ThenInclude(hs => hs.Hero)
                .ToListAsync();
        }

        public static async Task<bool> RegisterAsync(string username, string password, string playerName)
        {
            using var context = new HeroArenaContext();
            if (await context.Logins.AnyAsync(l => l.Username == username))
            {
                return false;
            }

            var login = new Login
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            };

            context.Logins.Add(login);
            await context.SaveChangesAsync();

            var player = new Player
            {
                Name = playerName,
                LoginID = login.ID
            };
            context.Players.Add(player);
            await context.SaveChangesAsync();

            return true;
        }

        public static async Task UpdatePlayerScoreAsync(int playerId, int score)
        {
            if (AppSettings.Instance.CurrentPlayer != null && AppSettings.Instance.CurrentPlayer.ID == playerId)
            {
                AppSettings.Instance.CurrentPlayer.Score = score;
            }
            await Task.CompletedTask;
        }

        public static async Task<bool> SeedDataAsync()
        {
            try
            {
                using var context = new HeroArenaContext();
                if (await context.Logins.AnyAsync())
                {
                    return false;
                }

                var adminLogin = new Login { Username = "admin", PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123") };
                var player1Login = new Login { Username = "test", PasswordHash = BCrypt.Net.BCrypt.HashPassword("test") };

                context.Logins.AddRange(adminLogin, player1Login);
                await context.SaveChangesAsync();

                var adminPlayer = new Player { Name = "Administrateur", LoginID = adminLogin.ID };
                var p1Player = new Player { Name = "Joueur Un", LoginID = player1Login.ID };

                context.Players.AddRange(adminPlayer, p1Player);
                await context.SaveChangesAsync();

                var existingHeroes = await context.Heroes.ToListAsync();

                foreach (var hero in existingHeroes)
                {
                    context.PlayerHeroes.Add(new PlayerHero { PlayerID = adminPlayer.ID, HeroID = hero.ID });
                    context.PlayerHeroes.Add(new PlayerHero { PlayerID = p1Player.ID, HeroID = hero.ID });
                }

                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
