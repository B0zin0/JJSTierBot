using Discord;
using Discord.WebSocket;
using JJSTierBot.Commands;
using JJSTierBot.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace JJSTierBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
            if (string.IsNullOrEmpty(token))
            { Console.WriteLine("ERROR: DISCORD_TOKEN not set."); return; }

            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds
                               | GatewayIntents.GuildMessages
            };

            var client   = new DiscordSocketClient(config);
            var data     = new DataService();
            var renderer = new TierListRenderer(data);
            var tierList = new TierListService(client, data, renderer);
            var cmds     = new SlashCommands(data, tierList, renderer, client);

            client.Log   += msg => { Console.WriteLine($"[{msg.Severity}] {msg.Message}"); return Task.CompletedTask; };

            client.Ready += async () =>
            {
                Console.WriteLine($"JJS Tier Bot online as {client.CurrentUser.Username}");
                await client.SetGameAsync("JJS Madness Tournament", type: ActivityType.Watching);

                foreach (var guild in client.Guilds)
                    foreach (var cmd in SlashCommands.GetCommandDefinitions())
                        await guild.CreateApplicationCommandAsync(cmd);
            };

            client.SlashCommandExecuted += cmds.Handle;

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            await Task.Delay(Timeout.Infinite);
        }
    }
}
