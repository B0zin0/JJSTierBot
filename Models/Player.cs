namespace JJSTierBot.Models
{
    public class Player
    {
        public string Name       { get; set; } = "";
        public string RobloxUser { get; set; } = "";
        public string Rank       { get; set; } = "Unranked";
        public bool   Retired    { get; set; } = false;
        public string AddedAt    { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");
        public int    Wins       { get; set; } = 0;
        public int    Losses     { get; set; } = 0;
        public List<string> RankHistory { get; set; } = new();
    }
}
