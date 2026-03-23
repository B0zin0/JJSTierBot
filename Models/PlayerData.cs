using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace JesseDex.Models
{
    public class PlayerData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        public ulong UserId   { get; set; }
        public string Username { get; set; } = "";

        public List<string> CaughtCharacterIds { get; set; } = new();

        public int TotalCaught => CaughtCharacterIds.Count;
    }
}
