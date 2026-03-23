using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JesseDex.Models
{
    public class Character
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        public string Name        { get; set; } = "";
        public string Description { get; set; } = "";
        public string ImageUrl    { get; set; } = "";
        public string Rarity      { get; set; } = "Common";
        public string Season      { get; set; } = "S1";
        public int    SpawnWeight  { get; set; } = 100;
    }
}
