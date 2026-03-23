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
            var activePlayers = _data.Data.Players.Count(p => !p.Retired);
            var lastUpdated   = DateTime.UtcNow.ToString("MMM dd, yyyy HH:mm") + " UTC";

            var builder = new EmbedBuilder()
                .WithTitle("⚔️  JJS Madness Tournament")
                .WithDescription(
                    $"**{activePlayers} active competitors** · Last updated: {lastUpdated}\n" +
                    "─────────────────────────────────────\n" +
                    "To reach **A+** beat the two lowest A+ players.\n" +
                    "**S and A+** require beating everyone below you in tier.\n" +
                    "**B tier and below** can skip ahead.\n" +
                    "─────────────────────────────────────")
                .WithColor(0xFFD700)
                .WithTimestamp(DateTimeOffset.UtcNow)
                .WithFooter("JJS Tier Bot · Tournament Tier List");

            foreach (var rank in DataService.RankOrder)
            {
                var players = _data.GetByRank(rank);
                var emoji   = DataService.RankEmoji(rank);
                var label   = DataService.RankLabel(rank);

                if (players.Count == 0)
                {
                    builder.AddField(
                        $"{emoji}  {label}",
                        "```\nEmpty\n```",
                        inline: false);
                }
                else
                {
                    var lines = players.Select((p, i) =>
                    {
                        var robloxPart = !string.IsNullOrEmpty(p.RobloxUser)
                            ? $"  [{p.RobloxUser}]({DataService.RobloxProfileUrl(p.RobloxUser)})"
                            : "";
                        var record = $"  `{p.Wins}W-{p.Losses}L`";
                        return $"**{i + 1}.** {p.Name}{robloxPart}{record}";
                    });

                    builder.AddField(
                        $"{emoji}  {label}  ({players.Count})",
                        string.Join("\n", lines),
                        inline: false);
                }
            }

            var retired = _data.Data.Players.Where(p => p.Retired).ToList();
            if (retired.Count > 0)
            {
                var lines = retired.Select(p =>
                    $"🎖️ {p.Name}  `{p.Wins}W-{p.Losses}L`");
                builder.AddField(
                    $"🎖️  RETIRED  ({retired.Count})",
                    string.Join("\n", lines),
                    inline: false);
            }

            return builder.Build();
        }

        public Embed BuildAllTimeEmbed()
        {
            var allPlayers = _data.Data.Players
                .OrderByDescending(p => p.Wins)
                .ThenBy(p => p.Losses)
                .ToList();

            var builder = new EmbedBuilder()
                .WithTitle("🏆  JJS Madness — All Time Records")
                .WithDescription(
                    $"**{allPlayers.Count} total players** tracked all time\n" +
                    "─────────────────────────────────────")
                .WithColor(0xFFD700)
                .WithTimestamp(DateTimeOffset.UtcNow)
                .WithFooter("JJS Tier Bot · All Time Records");

            if (allPlayers.Count == 0)
            {
                builder.AddField("No data yet", "Start logging challenges with /challenge");
                return builder.Build();
            }

            var lines = allPlayers.Select((p, i) =>
            {
                var medal = i switch { 0 => "🥇", 1 => "🥈", 2 => "🥉", _ => $"`#{i + 1}`" };
                var status = p.Retired ? " 🎖️" : $" {DataService.RankEmoji(p.Rank)}";
                var total  = p.Wins + p.Losses;
                var wr     = total > 0 ? $"{(p.Wins * 100 / total)}%" : "0%";
                var history = p.RankHistory.Count > 0
                    ? $"  Peak: **{p.RankHistory.OrderBy(r => Array.IndexOf(DataService.RankOrder, r)).FirstOrDefault() ?? p.Rank}**"
                    : "";
                return $"{medal} **{p.Name}**{status}  `{p.Wins}W-{p.Losses}L`  WR: {wr}{history}";
            });

            var chunks = lines.Chunk(10).ToList();
            for (int i = 0; i < chunks.Count; i++)
            {
                builder.AddField(
                    i == 0 ? "Rankings" : "​",
                    string.Join("\n", chunks[i]),
                    inline: false);
            }

            return builder.Build();
        }

        public Embed BuildChangeEmbed(string action, Player player,
            string oldRank, string changedBy)
        {
            return new EmbedBuilder()
                .WithTitle($"📋  Tier List Updated — {action}")
                .WithColor(DataService.RankColor(player.Rank))
                .AddField("Player", player.Name, inline: true)
                .AddField("Change", oldRank == ""
                    ? $"Added to {player.Rank}"
                    : $"{DataService.RankEmoji(oldRank)} {oldRank}  →  {DataService.RankEmoji(player.Rank)} {player.Rank}",
                    inline: true)
                .AddField("By", changedBy, inline: true)
                .WithTimestamp(DateTimeOffset.UtcNow)
                .WithFooter("JJS Tier Bot")
                .Build();
        }
    }
}
