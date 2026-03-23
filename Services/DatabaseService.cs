using MongoDB.Driver;
using JesseDex.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JesseDex.Services
{
    public class DatabaseService
    {
        private readonly IMongoCollection<PlayerData> _players;

        public DatabaseService(string connectionString)
        {
            var client   = new MongoClient(connectionString);
            var database = client.GetDatabase("jessedex");
            _players     = database.GetCollection<PlayerData>("players");

            var indexKeys = Builders<PlayerData>.IndexKeys.Ascending(p => p.UserId);
            _players.Indexes.CreateOne(new CreateIndexModel<PlayerData>(indexKeys,
                new CreateIndexOptions { Unique = true }));
        }

        public async Task<PlayerData> GetOrCreatePlayer(ulong userId, string username)
        {
            var player = await _players
                .Find(p => p.UserId == userId)
                .FirstOrDefaultAsync();

            if (player == null)
            {
                player = new PlayerData { UserId = userId, Username = username };
                await _players.InsertOneAsync(player);
            }

            return player;
        }

        public async Task AddCharacterToPlayer(ulong userId, string characterId)
        {
            var update = Builders<PlayerData>.Update
                .AddToSet(p => p.CaughtCharacterIds, characterId);
            await _players.UpdateOneAsync(p => p.UserId == userId, update);
        }

        public async Task<List<PlayerData>> GetLeaderboard(int count = 10)
        {
            return await _players
                .Find(_ => true)
                .SortByDescending(p => p.TotalCaught)
                .Limit(count)
                .ToListAsync();
        }

        public async Task UpdateUsername(ulong userId, string username)
        {
            var update = Builders<PlayerData>.Update.Set(p => p.Username, username);
            await _players.UpdateOneAsync(p => p.UserId == userId, update);
        }
    }
}
