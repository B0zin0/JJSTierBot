using System.Collections.Generic;

namespace JJSTierBot.Models
{
    public class TierData
    {
        public List<Player>       Players { get; set; } = new();
        public List<HistoryEntry> History { get; set; } = new();
        public ulong PinnedMessageId      { get; set; } = 0;
        public ulong TierListChannelId    { get; set; } = 0;
        public ulong LogChannelId         { get; set; } = 0;
    }
}
