namespace JJSTierBot.Models
{
    public class Player
    {
        public string Name       { get; set; } = "";
        public string RobloxUser { get; set; } = "";
        public string Rank       { get; set; } = "Unranked";
        public bool   Retired    { get; set; } = false;
        public string AddedAt    { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");
    }
}
