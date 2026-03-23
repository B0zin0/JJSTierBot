using Discord;
using Discord.WebSocket;
using JJSTierBot.Models;

namespace JJSTierBot.Services
{
    public class TierListService
    {
        private readonly DiscordSocketClient _client;
        private readonly DataService         _data;
        private readonly TierListRenderer    _renderer;

        public TierListService(DiscordSocketClient client, DataService data,
            TierListRenderer renderer)
        {
            _client   = client;
            _data     = data;
            _renderer = renderer;
        }

        public async Task UpdatePinnedList()
        {
            var channelId = _data.Data.TierListChannelId;
            if (channelId == 0) return;

            if (_client.GetChannel(channelId) is not ITextChannel channel) return;

            var embed = _renderer.BuildTierListEmbed();

            if (_data.Data.PinnedMessageId != 0)
            {
                try
                {
                    var msg = await channel.GetMessageAsync(_data.Data.PinnedMessageId)
                              as IUserMessage;
                    if (msg != null)
                    {
                        await msg.ModifyAsync(p => p.Embed = embed);
                        return;
                    }
                }
                catch { }
            }

            var newMsg = await channel.SendMessageAsync(embed: embed);
            await newMsg.PinAsync();
            _data.Data.PinnedMessageId = newMsg.Id;
            _data.Save();
        }

        public async Task LogChange(string action, Player player,
            string oldRank, string changedBy)
        {
            var channelId = _data.Data.LogChannelId;
            if (channelId == 0) return;

            if (_client.GetChannel(channelId) is not ITextChannel channel) return;

            var embed = _renderer.BuildChangeEmbed(action, player, oldRank, changedBy);
            await channel.SendMessageAsync(embed: embed);
        }
    }
}
