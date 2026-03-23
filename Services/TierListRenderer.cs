using Discord;
using JJSTierBot.Models;

namespace JJSTierBot.Services
{
    public class TierListRenderer
    {
        private readonly DataService _data;

        public TierListRenderer(DataService data)
        {
            _data = data;
        }

        public Embed BuildTierListEmbed()
        {
            var builder = new EmbedBuilder()
                .WithTitle("JJS Madness Tournament — Tier List")
                .WithColor(0xFFAA00)
                .WithTimestamp(DateTimeOffset.UtcNow)
                .WithFooter("JJS Tier Bot — Updated");

            foreach (var rank in DataService.RankOrder)
            {
                var players = _data.GetByRank(rank);
                var emoji   = DataService.RankEmoji(rank);

                if (players.Count == 0)
                {
                    builder.AddField($"{emoji} {rank}", "*Empty*", inline: false);
                }
                else
                {
                    var lines = players.Select(p =>
                        $"• **{p.Name}**" +
                        (!string.IsNullOrEmpty(p.RobloxUser) ? $" — [{p.RobloxUser}]({DataService.RobloxProfileUrl(p.RobloxUser)})" : ""));
                    builder.AddField($"{emoji} {rank} ({players.Count})",
                        string.Join("\n", lines), inline: false);
                }
            }

            var retired = _data.Data.Players.Where(p => p.Retired).ToList();
            if (retired.Count > 0)
            {
                var lines = retired.Select(p => $"• {p.Name}");
                builder.AddField($"🎖️ Retired ({retired.Count})",
                    string.Join("\n", lines), inline: false);
            }

            var total = _data.Data.Players.Count(p => !p.Retired);
            builder.WithDescription(
                $"**{total} active players** across all tiers\n\n" +
                "To reach A+ you must beat the two lowest ranked A+ players.\n" +
                "S and A+ require beating everyone below you in that tier.\n" +
                "B tier and below can skip ahead.");

            return builder.Build();
        }

        public Embed BuildChangeEmbed(string action, Player player,
            string oldRank, string changedBy)
        {
            return new EmbedBuilder()
                .WithTitle($"Tier List Updated — {action}")
                .WithColor(DataService.RankColor(player.Rank))
                .AddField("Player",   player.Name,   inline: true)
                .AddField("Change",   oldRank == "" ? player.Rank : $"{oldRank} → {player.Rank}", inline: true)
                .AddField("By",       changedBy,     inline: true)
                .WithTimestamp(DateTimeOffset.UtcNow)
                .WithFooter("JJS Tier Bot")
                .Build();
        }
    }
}
