using System.Text.Json;
using JJSTierBot.Models;

namespace JJSTierBot.Services
{
    public class DataService
    {
        private static readonly string FilePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "tierdata.json");

        private TierData _data;

        public DataService()
        {
            _data = Load();
        }

        public TierData Data => _data;

        private TierData Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var json = File.ReadAllText(FilePath);
                    return JsonSerializer.Deserialize<TierData>(json) ?? new TierData();
                }
            }
            catch { }
            return new TierData();
        }

        public void Save()
        {
            var json = JsonSerializer.Serialize(_data,
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }

        public static readonly string[] RankOrder =
            { "S", "A+", "A", "B", "C", "D", "F", "Unranked" };

        public static bool IsValidRank(string rank) =>
            RankOrder.Contains(rank);

        public List<Player> GetByRank(string rank) =>
            _data.Players.Where(p => p.Rank == rank && !p.Retired).ToList();

        public Player? FindPlayer(string name) =>
            _data.Players.FirstOrDefault(p =>
                p.Name.ToLower() == name.ToLower() && !p.Retired);

        public Player? FindPlayerIncludeRetired(string name) =>
            _data.Players.FirstOrDefault(p =>
                p.Name.ToLower() == name.ToLower());

        public void AddHistory(string playerName, string oldRank,
            string newRank, string action, string changedBy)
        {
            _data.History.Insert(0, new HistoryEntry
            {
                PlayerName = playerName,
                OldRank    = oldRank,
                NewRank    = newRank,
                Action     = action,
                ChangedBy  = changedBy,
                Timestamp  = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm")
            });

            if (_data.History.Count > 100)
                _data.History = _data.History.Take(100).ToList();
        }

        public static string RobloxProfileUrl(string robloxUsername) =>
            $"https://www.roblox.com/users/profile?username={Uri.EscapeDataString(robloxUsername)}";

        public static string RankEmoji(string rank) => rank switch
        {
            "S"        => "👑",
            "A+"       => "🔴",
            "A"        => "🟠",
            "B"        => "🔵",
            "C"        => "🟢",
            "D"        => "⚪",
            "F"        => "💀",
            "Unranked" => "❔",
            "Retired"  => "🎖️",
            _          => "❔"
        };

        public static string RankLabel(string rank) => rank switch
        {
            "S"        => "S  — ELITE",
            "A+"       => "A+ — TOP TIER",
            "A"        => "A  — HIGH TIER",
            "B"        => "B  — MID TIER",
            "C"        => "C  — LOW MID",
            "D"        => "D  — LOW TIER",
            "F"        => "F  — BOTTOM",
            "Unranked" => "UNRANKED",
            _          => rank
        };

        public static uint RankColor(string rank) => rank switch
        {
            "S"        => 0xFFD700,
            "A+"       => 0xFF4444,
            "A"        => 0xFF8C00,
            "B"        => 0x4488FF,
            "C"        => 0x44CC44,
            "D"        => 0xAAAAAA,
            "F"        => 0x555555,
            "Unranked" => 0x333333,
            _          => 0x333333
        };
    }
}
