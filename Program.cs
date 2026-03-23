using Discord;
using Discord.WebSocket;
using JesseDex.Commands;
using JesseDex.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace JesseDex
{
    class Program
    {
        private static DiscordSocketClient _client = null!;
        private static DatabaseService     _db     = null!;
        private static SpawnService        _spawn  = null!;
        private static SlashCommands       _cmds   = null!;

        static async Task Main(string[] args)
        {
            var token      = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
            var mongoUri   = Environment.GetEnvironmentVariable("MONGODB_URI");
            var channelEnv = Environment.GetEnvironmentVariable("SPAWN_CHANNEL_ID");

            if (string.IsNullOrEmpty(token))   { Console.WriteLine("ERROR: DISCORD_TOKEN not set.");  return; }
            if (string.IsNullOrEmpty(mongoUri)) { Console.WriteLine("ERROR: MONGODB_URI not set.");   return; }

            ulong.TryParse(channelEnv, out var spawnChannelId);

            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds
                               | GatewayIntents.GuildMessages
                               | GatewayIntents.MessageContent
            };

            _client = new DiscordSocketClient(config);
            _db     = new DatabaseService(mongoUri);
            _spawn  = new SpawnService(_client, _db, spawnChannelId);
            _cmds   = new SlashCommands(_db, _spawn);

            _client.Log             += Log;
            _client.Ready           += Ready;
            _client.MessageReceived += OnMessage;
            _client.SlashCommandExecuted    += _cmds.Handle;
            _client.ButtonExecuted  += OnButton;

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            _spawn.Start();

            await Task.Delay(Timeout.Infinite);
        }

        private static Task Log(LogMessage msg)
        {
            Console.WriteLine($"[{msg.Severity}] {msg.Source}: {msg.Message}");
            return Task.CompletedTask;
        }

        private static async Task Ready()
        {
            Console.WriteLine($"JesseDex online as {_client.CurrentUser.Username}");
            await _client.SetGameAsync("Catch MCSM characters!", type: ActivityType.Playing);

            foreach (var guild in _client.Guilds)
                foreach (var cmd in SlashCommands.GetCommandDefinitions())
                    await guild.CreateApplicationCommandAsync(cmd);
        }

        private static async Task OnMessage(Discord.WebSocket.SocketMessage message)
        {
            await _spawn.OnMessageReceived(message);
        }

        private static async Task OnButton(SocketMessageComponent interaction)
        {
            if (!interaction.Data.CustomId.StartsWith("catch_")) return;
            await _spawn.HandleButtonPress(interaction);
        }
    }
}
