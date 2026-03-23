namespace JJSTierBot.Models
{
    public class HistoryEntry
    {
        public string Timestamp  { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm");
        public string PlayerName { get; set; } = "";
        public string OldRank    { get; set; } = "";
        public string NewRank    { get; set; } = "";
        public string Action     { get; set; } = "";
        public string ChangedBy  { get; set; } = "";
    }
}
