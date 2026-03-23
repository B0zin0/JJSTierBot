using Discord;
using Discord.WebSocket;
using JesseDex.Models;
using JesseDex.Services;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JesseDex.Commands
{
    public class SlashCommands
    {
        private readonly DatabaseService _db;
        private readonly SpawnService    _spawn;

        public SlashCommands(DatabaseService db, SpawnService spawn)
        {
            _db    = db;
            _spawn = spawn;
        }

        public async Task Handle(SocketSlashCommand command)
        {
            switch (command.CommandName)
            {
                case "inv":    await Inv(command);   break;
                case "dex":    await Dex(command);   break;
                case "char":   await Char(command);  break;
                case "lb":     await Lb(command);    break;
                case "trade":  await Trade(command); break;
                case "spawn":  await Spawn(command); break;
            }
        }

        private async Task Inv(SocketSlashCommand cmd)
        {
            var player = await _db.GetOrCreatePlayer(cmd.User.Id, cmd.User.Username);

            if (player.CaughtCharacterIds.Count == 0)
            {
                await cmd.RespondAsync(embed: new EmbedBuilder()
                    .WithTitle($"{cmd.User.Username}'s Collection")
                    .WithDescription("You haven't caught anyone yet! Wait for a character to spawn and be the first to pick the right name.")
                    .WithColor(0xFFAA00)
                    .WithFooter("JesseDex by B0zin0")
                    .Build(), ephemeral: true);
                return;
            }

            var caught = CharacterData.All
                .Where(c => player.CaughtCharacterIds.Contains(c.Id))
                .OrderBy(c => c.Season)
                .ThenBy(c => c.Rarity)
                .ToList();

            var sb = new StringBuilder();
            foreach (var c in caught)
            {
                var star = c.Rarity switch
                {
                    "Legendary" => "⭐",
                    "Epic"      => "💜",
                    "Rare"      => "💙",
                    _           => "⬜"
                };
                sb.AppendLine($"{star} **{c.Name}** — {c.Rarity} · {c.Season}");
            }

            await cmd.RespondAsync(embed: new EmbedBuilder()
                .WithTitle($"{cmd.User.Username}'s Collection")
                .WithDescription(sb.ToString())
                .WithColor(0xFFAA00)
                .AddField("Progress", $"{caught.Count} / {CharacterData.All.Count} characters", inline: true)
                .WithFooter("JesseDex by B0zin0")
                .Build(), ephemeral: true);
        }

        private async Task Dex(SocketSlashCommand cmd)
        {
            var player = await _db.GetOrCreatePlayer(cmd.User.Id, cmd.User.Username);

            var sb = new StringBuilder();
            foreach (var c in CharacterData.All.OrderBy(c => c.Season).ThenBy(c => c.Name))
            {
                var owned = player.CaughtCharacterIds.Contains(c.Id);
                var star  = c.Rarity switch
                {
                    "Legendary" => "⭐",
                    "Epic"      => "💜",
                    "Rare"      => "💙",
                    _           => "⬜"
                };
                sb.AppendLine(owned
                    ? $"{star} **{c.Name}** ✅ · {c.Season}"
                    : $"⬛ ??? · {c.Season}");
            }

            await cmd.RespondAsync(embed: new EmbedBuilder()
                .WithTitle("JesseDex — Full Character List")
                .WithDescription(sb.ToString())
                .WithColor(0xFFAA00)
                .AddField("Your progress", $"{player.TotalCaught} / {CharacterData.All.Count}")
                .WithFooter("Question marks mean you haven't caught them yet — JesseDex by B0zin0")
                .Build(), ephemeral: true);
        }

        private async Task Char(SocketSlashCommand cmd)
        {
            var name = cmd.Data.Options.FirstOrDefault()?.Value?.ToString()?.Trim();
            if (string.IsNullOrEmpty(name))
            {
                await cmd.RespondAsync("Please provide a character name.", ephemeral: true);
                return;
            }

            var character = CharacterData.All
                .FirstOrDefault(c => c.Name.ToLower() == name.ToLower());

            if (character == null)
            {
                await cmd.RespondAsync($"No character called **{name}** found in JesseDex.", ephemeral: true);
                return;
            }

            var player = await _db.GetOrCreatePlayer(cmd.User.Id, cmd.User.Username);
            var owned  = player.CaughtCharacterIds.Contains(character.Id);

            await cmd.RespondAsync(embed: new EmbedBuilder()
                .WithTitle(character.Name)
                .WithDescription(character.Description)
                .WithImageUrl(character.ImageUrl)
                .WithColor(CharacterData.RarityColorUint(character.Rarity))
                .AddField("Rarity",  character.Rarity,              inline: true)
                .AddField("Season",  character.Season,              inline: true)
                .AddField("Status",  owned ? "✅ You own this!" : "❌ Not caught yet", inline: true)
                .WithFooter("JesseDex by B0zin0")
                .Build());
        }

        private async Task Lb(SocketSlashCommand cmd)
        {
            var top = await _db.GetLeaderboard(10);

            if (top.Count == 0)
            {
                await cmd.RespondAsync("Nobody has caught anything yet — be the first!", ephemeral: true);
                return;
            }

            var sb = new StringBuilder();
            for (int i = 0; i < top.Count; i++)
            {
                var medal = i switch { 0 => "🥇", 1 => "🥈", 2 => "🥉", _ => $"#{i + 1}" };
                sb.AppendLine($"{medal} **{top[i].Username}** — {top[i].TotalCaught} caught");
            }

            await cmd.RespondAsync(embed: new EmbedBuilder()
                .WithTitle("JesseDex Leaderboard")
                .WithDescription(sb.ToString())
                .WithColor(0xFFAA00)
                .WithFooter("JesseDex by B0zin0 — keep catching to climb the ranks!")
                .Build());
        }

        private async Task Trade(SocketSlashCommand cmd)
        {
            await cmd.RespondAsync(embed: new EmbedBuilder()
                .WithTitle("Trading")
                .WithDescription("Trading is coming in a future update! For now, focus on catching — there are 20 characters to find.")
                .WithColor(0xFFAA00)
                .WithFooter("JesseDex by B0zin0")
                .Build(), ephemeral: true);
        }

        private async Task Spawn(SocketSlashCommand cmd)
        {
            if (cmd.User is not SocketGuildUser gUser || !gUser.GuildPermissions.Administrator)
            {
                await cmd.RespondAsync("Only admins can force a spawn.", ephemeral: true);
                return;
            }

            await cmd.RespondAsync("Spawning a character...", ephemeral: true);
            await _spawn.SpawnInChannel(cmd.Channel.Id);
        }

        public static SlashCommandProperties[] GetCommandDefinitions()
        {
            return new SlashCommandProperties[]
            {
                new SlashCommandBuilder()
                    .WithName("inv")
                    .WithDescription("View your JesseDex collection")
                    .Build(),

                new SlashCommandBuilder()
                    .WithName("dex")
                    .WithDescription("See all characters and which ones you're missing")
                    .Build(),

                new SlashCommandBuilder()
                    .WithName("char")
                    .WithDescription("Get info on a specific character")
                    .AddOption("name", ApplicationCommandOptionType.String, "Character name", isRequired: true)
                    .Build(),

                new SlashCommandBuilder()
                    .WithName("lb")
                    .WithDescription("Show the JesseDex leaderboard")
                    .Build(),

                new SlashCommandBuilder()
                    .WithName("trade")
                    .WithDescription("Trade characters with another player")
                    .Build(),

                new SlashCommandBuilder()
                    .WithName("spawn")
                    .WithDescription("Force a character to spawn (admin only)")
                    .Build(),
            };
        }
    }
}
