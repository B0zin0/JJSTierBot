using Discord;
using Discord.WebSocket;
using JJSTierBot.Models;
using JJSTierBot.Services;
using System.Text;

namespace JJSTierBot.Commands
{
    public class SlashCommands
    {
        private readonly DataService         _data;
        private readonly TierListService     _tierList;
        private readonly TierListRenderer    _renderer;
        private readonly DiscordSocketClient _client;

        public SlashCommands(DataService data, TierListService tierList,
            TierListRenderer renderer, DiscordSocketClient client)
        {
            _data     = data;
            _tierList = tierList;
            _renderer = renderer;
            _client   = client;
        }

        public async Task Handle(SocketSlashCommand cmd)
        {
            switch (cmd.CommandName)
            {
                case "add":       await Add(cmd);       break;
                case "remove":    await Remove(cmd);    break;
                case "retire":    await Retire(cmd);    break;
                case "rank":      await Rank(cmd);      break;
                case "list":      await List(cmd);      break;
                case "alltime":   await AllTime(cmd);   break;
                case "challenge": await Challenge(cmd); break;
                case "history":   await History(cmd);   break;
                case "stats":     await Stats(cmd);     break;
                case "setup":     await Setup(cmd);     break;
            }
        }

        private static bool IsAdmin(SocketSlashCommand cmd)
        {
            if (cmd.User is not SocketGuildUser user) return false;
            return user.GuildPermissions.Administrator ||
                   user.GuildPermissions.ManageGuild;
        }

        private async Task Add(SocketSlashCommand cmd)
        {
            if (!IsAdmin(cmd))
            { await cmd.RespondAsync("Only admins can do that.", ephemeral: true); return; }

            var name   = cmd.Data.Options.FirstOrDefault(o => o.Name == "name")?.Value?.ToString()?.Trim()   ?? "";
            var roblox = cmd.Data.Options.FirstOrDefault(o => o.Name == "roblox")?.Value?.ToString()?.Trim() ?? "";
            var rank   = cmd.Data.Options.FirstOrDefault(o => o.Name == "rank")?.Value?.ToString()           ?? "Unranked";

            if (string.IsNullOrEmpty(name))
            { await cmd.RespondAsync("Player name can't be empty.", ephemeral: true); return; }

            if (!DataService.IsValidRank(rank))
            { await cmd.RespondAsync($"Invalid rank. Valid: {string.Join(", ", DataService.RankOrder)}", ephemeral: true); return; }

            if (_data.FindPlayer(name) != null)
            { await cmd.RespondAsync($"**{name}** is already in the tier list.", ephemeral: true); return; }

            var player = new Player
            {
                Name       = name,
                RobloxUser = roblox,
                Rank       = rank,
                RankHistory = new List<string> { rank }
            };

            _data.Data.Players.Add(player);
            _data.AddHistory(name, "", rank, "Added", cmd.User.Username);
            _data.Save();

            await _tierList.UpdatePinnedList();
            await _tierList.LogChange("Player Added", player, "", cmd.User.Username);

            await cmd.RespondAsync(embed: new EmbedBuilder()
                .WithTitle("✅  Player Added")
                .WithColor(DataService.RankColor(rank))
                .AddField("Name",   name,                                    inline: true)
                .AddField("Rank",   $"{DataService.RankEmoji(rank)} {rank}", inline: true)
                .AddField("Roblox", roblox == "" ? "Not set" : roblox,       inline: true)
                .WithFooter("JJS Tier Bot")
                .Build(), ephemeral: true);
        }

        private async Task Remove(SocketSlashCommand cmd)
        {
            if (!IsAdmin(cmd))
            { await cmd.RespondAsync("Only admins can do that.", ephemeral: true); return; }

            var name   = cmd.Data.Options.FirstOrDefault()?.Value?.ToString()?.Trim() ?? "";
            var player = _data.FindPlayer(name);

            if (player == null)
            { await cmd.RespondAsync($"**{name}** wasn't found.", ephemeral: true); return; }

            var oldRank = player.Rank;
            _data.Data.Players.Remove(player);
            _data.AddHistory(name, oldRank, "", "Removed", cmd.User.Username);
            _data.Save();

            await _tierList.UpdatePinnedList();
            await _tierList.LogChange("Player Removed", player, oldRank, cmd.User.Username);

            await cmd.RespondAsync($"**{name}** removed from the tier list.", ephemeral: true);
        }

        private async Task Retire(SocketSlashCommand cmd)
        {
            if (!IsAdmin(cmd))
            { await cmd.RespondAsync("Only admins can do that.", ephemeral: true); return; }

            var name   = cmd.Data.Options.FirstOrDefault()?.Value?.ToString()?.Trim() ?? "";
            var player = _data.FindPlayer(name);

            if (player == null)
            { await cmd.RespondAsync($"**{name}** wasn't found.", ephemeral: true); return; }

            var oldRank    = player.Rank;
            player.Retired = true;
            player.Rank    = "Retired";
            _data.AddHistory(name, oldRank, "Retired", "Retired", cmd.User.Username);
            _data.Save();

            await _tierList.UpdatePinnedList();
            await _tierList.LogChange("Player Retired", player, oldRank, cmd.User.Username);

            await cmd.RespondAsync(embed: new EmbedBuilder()
                .WithTitle("🎖️  Player Retired")
                .WithColor(0xFFD700)
                .WithDescription($"**{name}** has been retired. Thanks for competing!")
                .AddField("Final Rank",   oldRank,                         inline: true)
                .AddField("Final Record", $"{player.Wins}W-{player.Losses}L", inline: true)
                .WithFooter("JJS Tier Bot")
                .Build(), ephemeral: true);
        }

        private async Task Rank(SocketSlashCommand cmd)
        {
            if (!IsAdmin(cmd))
            { await cmd.RespondAsync("Only admins can do that.", ephemeral: true); return; }

            var name    = cmd.Data.Options.FirstOrDefault(o => o.Name == "name")?.Value?.ToString()?.Trim() ?? "";
            var newRank = cmd.Data.Options.FirstOrDefault(o => o.Name == "rank")?.Value?.ToString()         ?? "";
            var player  = _data.FindPlayer(name);

            if (player == null)
            { await cmd.RespondAsync($"**{name}** wasn't found.", ephemeral: true); return; }

            if (!DataService.IsValidRank(newRank))
            { await cmd.RespondAsync($"Invalid rank. Valid: {string.Join(", ", DataService.RankOrder)}", ephemeral: true); return; }

            var oldRank = player.Rank;
            player.Rank = newRank;
            if (!player.RankHistory.Contains(newRank))
                player.RankHistory.Add(newRank);

            _data.AddHistory(name, oldRank, newRank, "Rank Change", cmd.User.Username);
            _data.Save();

            await _tierList.UpdatePinnedList();
            await _tierList.LogChange("Rank Changed", player, oldRank, cmd.User.Username);

            await cmd.RespondAsync(embed: new EmbedBuilder()
                .WithTitle("📈  Rank Updated")
                .WithColor(DataService.RankColor(newRank))
                .AddField("Player", name,                                          inline: true)
                .AddField("Before", $"{DataService.RankEmoji(oldRank)} {oldRank}", inline: true)
                .AddField("After",  $"{DataService.RankEmoji(newRank)} {newRank}", inline: true)
                .WithFooter("JJS Tier Bot")
                .Build(), ephemeral: true);
        }

        private async Task List(SocketSlashCommand cmd)
        {
            await cmd.RespondAsync(
                embed: _renderer.BuildTierListEmbed(),
                ephemeral: false);

            _ = Task.Run(async () =>
            {
                await Task.Delay(10 * 60 * 1000);
                try { await cmd.DeleteOriginalResponseAsync(); } catch { }
            });
        }

        private async Task AllTime(SocketSlashCommand cmd)
        {
            await cmd.RespondAsync(embed: _renderer.BuildAllTimeEmbed());

            _ = Task.Run(async () =>
            {
                await Task.Delay(10 * 60 * 1000);
                try { await cmd.DeleteOriginalResponseAsync(); } catch { }
            });
        }

        private async Task Challenge(SocketSlashCommand cmd)
        {
            if (!IsAdmin(cmd))
            { await cmd.RespondAsync("Only admins can log challenges.", ephemeral: true); return; }

            var winner = cmd.Data.Options.FirstOrDefault(o => o.Name == "winner")?.Value?.ToString()?.Trim() ?? "";
            var loser  = cmd.Data.Options.FirstOrDefault(o => o.Name == "loser")?.Value?.ToString()?.Trim()  ?? "";
            var notes  = cmd.Data.Options.FirstOrDefault(o => o.Name == "notes")?.Value?.ToString()?.Trim()  ?? "";

            var winnerPlayer = _data.FindPlayer(winner);
            var loserPlayer  = _data.FindPlayer(loser);

            if (winnerPlayer == null)
            { await cmd.RespondAsync($"**{winner}** wasn't found.", ephemeral: true); return; }
            if (loserPlayer == null)
            { await cmd.RespondAsync($"**{loser}** wasn't found.", ephemeral: true); return; }

            winnerPlayer.Wins++;
            loserPlayer.Losses++;

            _data.AddHistory(winner, winnerPlayer.Rank, winnerPlayer.Rank,
                $"Beat {loser}", cmd.User.Username);
            _data.Save();

            await _tierList.UpdatePinnedList();

            var logChannelId = _data.Data.LogChannelId;
            if (logChannelId != 0 &&
                _client.GetChannel(logChannelId) is ITextChannel logChan)
            {
                await logChan.SendMessageAsync(embed: new EmbedBuilder()
                    .WithTitle("⚔️  Challenge Result")
                    .WithColor(0xFFD700)
                    .AddField("🏆 Winner",
                        $"**{winner}**\n{DataService.RankEmoji(winnerPlayer.Rank)} {winnerPlayer.Rank}\n`{winnerPlayer.Wins}W-{winnerPlayer.Losses}L`",
                        inline: true)
                    .AddField("💀 Loser",
                        $"**{loser}**\n{DataService.RankEmoji(loserPlayer.Rank)} {loserPlayer.Rank}\n`{loserPlayer.Wins}W-{loserPlayer.Losses}L`",
                        inline: true)
                    .AddField("Notes", notes == "" ? "None" : notes)
                    .WithTimestamp(DateTimeOffset.UtcNow)
                    .WithFooter($"Logged by {cmd.User.Username} — JJS Tier Bot")
                    .Build());
            }

            await cmd.RespondAsync(embed: new EmbedBuilder()
                .WithTitle("✅  Challenge Logged")
                .WithColor(0xFFD700)
                .AddField("Winner", $"{winner}  `{winnerPlayer.Wins}W-{winnerPlayer.Losses}L`", inline: true)
                .AddField("Loser",  $"{loser}  `{loserPlayer.Wins}W-{loserPlayer.Losses}L`",   inline: true)
                .WithFooter("JJS Tier Bot")
                .Build(), ephemeral: true);
        }

        private async Task History(SocketSlashCommand cmd)
        {
            var entries = _data.Data.History.Take(15).ToList();

            if (entries.Count == 0)
            { await cmd.RespondAsync("No history yet.", ephemeral: true); return; }

            var sb = new StringBuilder();
            foreach (var e in entries)
                sb.AppendLine($"`{e.Timestamp}` **{e.PlayerName}** — {e.Action}" +
                    (e.OldRank != "" && e.NewRank != "" && e.OldRank != e.NewRank
                        ? $" ({e.OldRank} → {e.NewRank})" : "") +
                    $" · {e.ChangedBy}");

            await cmd.RespondAsync(embed: new EmbedBuilder()
                .WithTitle("📋  Recent Changes")
                .WithDescription(sb.ToString())
                .WithColor(0xFFD700)
                .WithFooter("Showing last 15 changes — JJS Tier Bot")
                .Build(), ephemeral: true);
        }

        private async Task Stats(SocketSlashCommand cmd)
        {
            var name   = cmd.Data.Options.FirstOrDefault()?.Value?.ToString()?.Trim() ?? "";
            var player = _data.FindPlayerIncludeRetired(name);

            if (player == null)
            { await cmd.RespondAsync($"**{name}** wasn't found.", ephemeral: true); return; }

            var total  = player.Wins + player.Losses;
            var wr     = total > 0 ? $"{(player.Wins * 100 / total)}%" : "N/A";
            var peak   = player.RankHistory.Count > 0
                ? player.RankHistory.OrderBy(r =>
                    Array.IndexOf(DataService.RankOrder, r)).First()
                : player.Rank;

            var rankHistoryStr = player.RankHistory.Count > 0
                ? string.Join(" → ", player.RankHistory)
                : "No rank changes yet";

            await cmd.RespondAsync(embed: new EmbedBuilder()
                .WithTitle($"📊  {player.Name}'s Stats")
                .WithColor(DataService.RankColor(player.Rank))
                .AddField("Current Rank", $"{DataService.RankEmoji(player.Rank)} {player.Rank}", inline: true)
                .AddField("Record",       $"{player.Wins}W - {player.Losses}L",                  inline: true)
                .AddField("Win Rate",     wr,                                                      inline: true)
                .AddField("Peak Rank",    $"{DataService.RankEmoji(peak)} {peak}",                inline: true)
                .AddField("Status",       player.Retired ? "🎖️ Retired" : "⚔️ Active",           inline: true)
                .AddField("Joined",       player.AddedAt,                                          inline: true)
                .AddField("Rank History", rankHistoryStr)
                .WithFooter("JJS Tier Bot · All Time Stats")
                .Build());
        }

        private async Task Setup(SocketSlashCommand cmd)
        {
            if (!IsAdmin(cmd))
            { await cmd.RespondAsync("Only admins can set up the bot.", ephemeral: true); return; }

            var tierChan = cmd.Data.Options.FirstOrDefault(o => o.Name == "tierchannel")?.Value as IChannel;
            var logChan  = cmd.Data.Options.FirstOrDefault(o => o.Name == "logchannel")?.Value  as IChannel;

            if (tierChan != null) _data.Data.TierListChannelId = tierChan.Id;
            if (logChan  != null) _data.Data.LogChannelId      = logChan.Id;
            _data.Data.PinnedMessageId        = 0;
            _data.Data.AllTimePinnedMessageId = 0;
            _data.Save();

            await _tierList.UpdatePinnedList();

            await cmd.RespondAsync(embed: new EmbedBuilder()
                .WithTitle("✅  Setup Complete")
                .WithColor(0xFFD700)
                .AddField("Tier List Channel", tierChan?.Name ?? "Not set", inline: true)
                .AddField("Log Channel",       logChan?.Name  ?? "Not set", inline: true)
                .WithFooter("JJS Tier Bot — ready to go!")
                .Build(), ephemeral: true);
        }

        public static SlashCommandProperties[] GetCommandDefinitions()
        {
            return new[]
            {
                new SlashCommandBuilder()
                    .WithName("add")
                    .WithDescription("Add a player to the tier list (admin only)")
                    .AddOption("name",   ApplicationCommandOptionType.String, "Player name",     isRequired: true)
                    .AddOption("rank",   ApplicationCommandOptionType.String, "Starting rank",   isRequired: true)
                    .AddOption("roblox", ApplicationCommandOptionType.String, "Roblox username", isRequired: false)
                    .Build(),

                new SlashCommandBuilder()
                    .WithName("remove")
                    .WithDescription("Remove a player (admin only)")
                    .AddOption("name", ApplicationCommandOptionType.String, "Player name", isRequired: true)
                    .Build(),

                new SlashCommandBuilder()
                    .WithName("retire")
                    .WithDescription("Retire a player (admin only)")
                    .AddOption("name", ApplicationCommandOptionType.String, "Player name", isRequired: true)
                    .Build(),

                new SlashCommandBuilder()
                    .WithName("rank")
                    .WithDescription("Change a player's rank (admin only)")
                    .AddOption("name", ApplicationCommandOptionType.String, "Player name",                        isRequired: true)
                    .AddOption("rank", ApplicationCommandOptionType.String, "New rank (S/A+/A/B/C/D/F/Unranked)", isRequired: true)
                    .Build(),

                new SlashCommandBuilder()
                    .WithName("list")
                    .WithDescription("Show the current tier list (disappears after 10 minutes)")
                    .Build(),

                new SlashCommandBuilder()
                    .WithName("alltime")
                    .WithDescription("Show all time win/loss records")
                    .Build(),

                new SlashCommandBuilder()
                    .WithName("challenge")
                    .WithDescription("Log a challenge result (admin only)")
                    .AddOption("winner", ApplicationCommandOptionType.String, "Winner's name", isRequired: true)
                    .AddOption("loser",  ApplicationCommandOptionType.String, "Loser's name",  isRequired: true)
                    .AddOption("notes",  ApplicationCommandOptionType.String, "Extra notes",   isRequired: false)
                    .Build(),

                new SlashCommandBuilder()
                    .WithName("history")
                    .WithDescription("Show recent tier list changes")
                    .Build(),

                new SlashCommandBuilder()
                    .WithName("stats")
                    .WithDescription("Show a player's full stats and rank history")
                    .AddOption("name", ApplicationCommandOptionType.String, "Player name", isRequired: true)
                    .Build(),

                new SlashCommandBuilder()
                    .WithName("setup")
                    .WithDescription("Set up tier list and log channels (admin only)")
                    .AddOption("tierchannel", ApplicationCommandOptionType.Channel, "Where to post the tier list", isRequired: true)
                    .AddOption("logchannel",  ApplicationCommandOptionType.Channel, "Where to log changes",        isRequired: true)
                    .Build(),
            };
        }
    }
}
