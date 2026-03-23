using Discord;
using Discord.WebSocket;
using JesseDex.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JesseDex.Services
{
    public class SpawnService
    {
        private readonly DiscordSocketClient _client;
        private readonly DatabaseService     _db;
        private readonly Random              _rng = new();

        private readonly Dictionary<ulong, ActiveSpawn> _activeSpawns = new();
        private readonly Dictionary<ulong, int>         _chatCounter  = new();

        private ulong _spawnChannelId;

        private const int ChatThreshold  = 15;
        private const int TimedIntervalMs = 20 * 60 * 1000;

        public SpawnService(DiscordSocketClient client, DatabaseService db, ulong spawnChannelId)
        {
            _client         = client;
            _db             = db;
            _spawnChannelId = spawnChannelId;
        }

        public void Start()
        {
            _ = Task.Run(TimedSpawnLoop);
        }

        private async Task TimedSpawnLoop()
        {
            while (true)
            {
                await Task.Delay(TimedIntervalMs);
                await SpawnInChannel(_spawnChannelId);
            }
        }

        public async Task OnMessageReceived(SocketMessage message)
        {
            if (message.Author.IsBot) return;
            if (message.Channel is not SocketGuildChannel guildChannel) return;

            var channelId = guildChannel.Id;

            if (!_chatCounter.ContainsKey(channelId))
                _chatCounter[channelId] = 0;

            _chatCounter[channelId]++;

            if (_chatCounter[channelId] >= ChatThreshold
                && !_activeSpawns.ContainsKey(channelId)
                && _rng.Next(100) < 40)
            {
                _chatCounter[channelId] = 0;
                await SpawnInChannel(channelId);
            }
        }

        public async Task SpawnInChannel(ulong channelId)
        {
            if (_activeSpawns.ContainsKey(channelId)) return;

            if (_client.GetChannel(channelId) is not ITextChannel channel) return;

            var character = CharacterData.GetWeightedRandom();

            var allNames   = CharacterData.All.Select(c => c.Name).ToList();
            var wrongNames = allNames.Where(n => n != character.Name)
                                     .OrderBy(_ => _rng.Next())
                                     .Take(2)
                                     .ToList();

            var choices = new List<string> { character.Name }.Concat(wrongNames)
                                                              .OrderBy(_ => _rng.Next())
                                                              .ToArray();

            var embed = new EmbedBuilder()
                .WithTitle("A wild character appeared!")
                .WithDescription("Who is this MCSM character? Pick the right name to catch them!")
                .WithImageUrl(character.ImageUrl)
                .WithColor(CharacterData.RarityColorUint(character.Rarity))
                .AddField("Rarity", character.Rarity, inline: true)
                .AddField("Season", character.Season, inline: true)
                .WithFooter("First to pick the right name catches them! — JesseDex by B0zin0")
                .Build();

            var components = new ComponentBuilder()
                .WithButton(choices[0], $"catch_{channelId}_{choices[0]}", ButtonStyle.Primary)
                .WithButton(choices[1], $"catch_{channelId}_{choices[1]}", ButtonStyle.Primary)
                .WithButton(choices[2], $"catch_{channelId}_{choices[2]}", ButtonStyle.Primary)
                .Build();

            var msg = await channel.SendMessageAsync(embed: embed, components: components);

            _activeSpawns[channelId] = new ActiveSpawn
            {
                Character     = character,
                ChannelId     = channelId,
                MessageId     = msg.Id,
                CorrectAnswer = character.Name,
                Choices       = choices
            };

            _ = Task.Run(async () =>
            {
                await Task.Delay(60_000);
                if (_activeSpawns.TryGetValue(channelId, out var spawn) && !spawn.Caught)
                {
                    _activeSpawns.Remove(channelId);
                    try
                    {
                        var m = await channel.GetMessageAsync(msg.Id) as IUserMessage;
                        if (m != null)
                            await m.ModifyAsync(p =>
                            {
                                p.Embed = new EmbedBuilder()
                                    .WithTitle("They got away!")
                                    .WithDescription($"Nobody caught **{character.Name}** in time.")
                                    .WithColor(Color.DarkGrey)
                                    .Build();
                                p.Components = new ComponentBuilder().Build();
                            });
                    }
                    catch { }
                }
            });
        }

        public async Task<string?> HandleButtonPress(SocketMessageComponent interaction)
        {
            var parts     = interaction.Data.CustomId.Split('_', 3);
            if (parts.Length < 3) return null;

            if (!ulong.TryParse(parts[1], out var channelId)) return null;
            var chosen = parts[2];

            if (!_activeSpawns.TryGetValue(channelId, out var spawn)) return null;
            if (spawn.Caught) return null;

            var userId   = interaction.User.Id;
            var username = interaction.User.Username;

            if (chosen != spawn.CorrectAnswer)
            {
                await interaction.RespondAsync(
                    $"Wrong! That's not the right character. Try again before someone else gets them!",
                    ephemeral: true);
                return null;
            }

            spawn.Caught = true;
            _activeSpawns.Remove(channelId);

            await _db.AddCharacterToPlayer(userId, spawn.Character.Id);
            await _db.UpdateUsername(userId, username);

            if (interaction.Channel is ITextChannel chan)
            {
                var m = await chan.GetMessageAsync(spawn.MessageId) as IUserMessage;
                if (m != null)
                    await m.ModifyAsync(p =>
                    {
                        p.Embed = new EmbedBuilder()
                            .WithTitle($"{interaction.User.Username} caught {spawn.Character.Name}!")
                            .WithDescription(spawn.Character.Description)
                            .WithImageUrl(spawn.Character.ImageUrl)
                            .WithColor(CharacterData.RarityColorUint(spawn.Character.Rarity))
                            .AddField("Rarity", spawn.Character.Rarity, inline: true)
                            .AddField("Season", spawn.Character.Season, inline: true)
                            .WithFooter("JesseDex — MCSM Character Collector by B0zin0")
                            .Build();
                        p.Components = new ComponentBuilder().Build();
                    });
            }

            return spawn.Character.Name;
        }
    }
}
