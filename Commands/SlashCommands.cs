using Discord;
using Discord.WebSocket;
using JJSTierBot.Models;
using JJSTierBot.Services;
using System.Text;

namespace JJSTierBot.Commands
{
    public class SlashCommands
    {
        private readonly DataService      _data;
        private readonly TierListService  _tierList;
        private readonly TierListRenderer _renderer;
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
                case "challenge": await Challenge(cmd); break;
                case "history":   await History(cmd);   break;
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
            { await cmd.RespondAsync($"Invalid rank. Valid ranks: {string.Join(", ", DataService.RankOrder)}", ephemeral: true); return; }

            if (_data.FindPlayer(name) != null)
            { await cmd.RespondAsync($"**{name}** is already in the tier list.", ephemeral: true); return; }

            var player = new Player { Name = name, RobloxUser = roblox, Rank = rank };
            _data.Data.Players.Add(player);
            _data.AddHistory(name, "", rank, "Added", cmd.User.Username);
            _data.Save();

            await _tierList.UpdatePinnedList();
            await _tierList.LogChange("Player Added", player, "", cmd.User.Username);

            await cmd.RespondAsync(embed: new EmbedBuilder()
                .WithTitle("Player Added")
                .WithColor(DataService.RankColor(rank))
                .AddField("Name",   name,                                        inline: true)
                .AddField("Rank",   $"{DataService.RankEmoji(rank)} {rank}",     inline: true)
                .AddField("Roblox", roblox == "" ? "Not set" : roblox,           inline: true)
                .WithFooter("JJS Tier Bot")
                .Build());
        }

        private async Task Remove(SocketSlashCommand cmd)
        {
            if (!IsAdmin(cmd))
            { await cmd.RespondAsync("Only admins can do that.", ephemeral: true); return; }

            var name   = cmd.Data.Options.FirstOrDefault()?.Value?.ToString()?.Trim() ?? "";
            var player = _data.FindPlayer(name);

            if (player == null)
            { await cmd.RespondAsync($"**{name}** wasn't found in the tier list.", ephemeral: true); return; }

            _data.Data.Players.Remove(player);
            _data.AddHistory(name, player.Rank, "", "Removed", cmd.User.Username);
            _data.Save();

            await _tierList.UpdatePinnedList();
            await _tierList.LogChange("Player Removed", player, player.Rank, cmd.User.Username);

            await cmd.RespondAsync($"**{name}** has been removed from the tier list.");
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
                .WithTitle("Player Retired")
                .WithColor(0xFFAA00)
                .WithDescription($"**{name}** has been retired from the tournament. Thanks for competing!")
                .WithFooter("JJS Tier Bot")
                .Build());
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
            _data.AddHistory(name, oldRank, newRank, "Rank Change", cmd.User.Username);
            _data.Save();

            await _tierList.UpdatePinnedList();
            await _tierList.LogChange("Rank Changed", player, oldRank, cmd.User.Username);

            await cmd.RespondAsync(embed: new EmbedBuilder()
                .WithTitle("Rank Updated")
                .WithColor(DataService.RankColor(newRank))
                .AddField("Player", name,                                         inline: true)
                .AddField("Old",    $"{DataService.RankEmoji(oldRank)} {oldRank}", inline: true)
                .AddField("New",    $"{DataService.RankEmoji(newRank)} {newRank}", inline: true)
                .WithFooter("JJS Tier Bot")
                .Build());
        }

        private async Task List(SocketSlashCommand cmd)
        {
            await cmd.RespondAsync(embed: _renderer.BuildTierListEmbed());
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

            _data.AddHistory(winner, winnerPlayer.Rank, winnerPlayer.Rank,
                $"Beat {loser}", cmd.User.Username);
            _data.Save();

            var logChannelId = _data.Data.LogChannelId;
            if (logChannelId != 0 &&
                _client.GetChannel(logChannelId) is ITextChannel logChan)
            {
                await logChan.SendMessageAsync(embed: new EmbedBuilder()
                    .WithTitle("Challenge Result")
                    .WithColor(0xFFAA00)
                    .AddField("Winner", $"**{winner}** ({DataService.RankEmoji(winnerPlayer.Rank)} {winnerPlayer.Rank})", inline: true)
                    .AddField("Loser",  $"**{loser}** ({DataService.RankEmoji(loserPlayer.Rank)} {loserPlayer.Rank})",   inline: true)
                    .AddField("Notes",  notes == "" ? "None" : notes)
                    .WithTimestamp(DateTimeOffset.UtcNow)
                    .WithFooter($"Logged by {cmd.User.Username} — JJS Tier Bot")
                    .Build());
            }

            await cmd.RespondAsync(embed: new EmbedBuilder()
                .WithTitle("Challenge Logged")
                .WithColor(0xFFAA00)
                .AddField("Winner", winner, inline: true)
                .AddField("Loser",  loser,  inline: true)
                .WithFooter("JJS Tier Bot")
                .Build());
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
                    $" by {e.ChangedBy}");

            await cmd.RespondAsync(embed: new EmbedBuilder()
                .WithTitle("Tier List History")
                .WithDescription(sb.ToString())
                .WithColor(0xFFAA00)
                .WithFooter("Showing last 15 changes — JJS Tier Bot")
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
            _data.Save();

            await _tierList.UpdatePinnedList();

            await cmd.RespondAsync(embed: new EmbedBuilder()
                .WithTitle("JJS Tier Bot Setup Complete")
                .WithColor(0xFFAA00)
                .AddField("Tier List Channel", tierChan?.Name ?? "Not set", inline: true)
                .AddField("Log Channel",       logChan?.Name  ?? "Not set", inline: true)
                .WithFooter("JJS Tier Bot — ready to go!")
                .Build());
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
                    .WithDescription("Remove a player from the tier list (admin only)")
                    .AddOption("name", ApplicationCommandOptionType.String, "Player name", isRequired: true)
                    .Build(),

                new SlashCommandBuilder()
                    .WithName("retire")
                    .WithDescription("Retire a player from the tournament (admin only)")
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
                    .WithDescription("Show the full tier list")
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
                    .WithName("setup")
                    .WithDescription("Set up tier list and log channels (admin only)")
                    .AddOption("tierchannel", ApplicationCommandOptionType.Channel, "Where to post the tier list", isRequired: true)
                    .AddOption("logchannel",  ApplicationCommandOptionType.Channel, "Where to log changes",        isRequired: true)
                    .Build(),
            };
        }
    }
}
