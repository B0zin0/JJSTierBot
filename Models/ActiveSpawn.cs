using Discord;

namespace JesseDex.Models
{
    public class ActiveSpawn
    {
        public Character  Character    { get; set; } = null!;
        public ulong      ChannelId    { get; set; }
        public ulong      MessageId    { get; set; }
        public string     CorrectAnswer { get; set; } = "";
        public string[]   Choices      { get; set; } = Array.Empty<string>();
        public bool       Caught       { get; set; } = false;
    }
}
