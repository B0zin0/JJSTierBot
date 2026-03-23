# JesseDex

MCSM character collector bot for Discord. Catch all 20 characters from Minecraft: Story Mode.

## Commands

| Command | Description |
|---|---|
| `/inv` | View your collection |
| `/dex` | See all characters and which you're missing |
| `/char [name]` | Info on a specific character |
| `/lb` | Leaderboard |
| `/spawn` | Force a spawn (admin only) |

## How it works

Characters spawn randomly when people chat (every ~15 messages) and automatically every 20 minutes. When one spawns, three buttons appear with name choices. First person to pick the right name catches the character!

## Rarities

- ⬜ Common
- 💙 Rare  
- 💜 Epic
- ⭐ Legendary

## Setup on Railway

1. Push to GitHub
2. New project on railway.app → connect repo
3. Add environment variables:
   - `DISCORD_TOKEN` — your bot token
   - `MONGODB_URI` — your MongoDB connection string
   - `SPAWN_CHANNEL_ID` — channel ID for timed spawns
4. Railway auto-deploys via Dockerfile

## MongoDB setup

1. Go to mongodb.com/atlas → create free cluster
2. Database Access → create a user
3. Network Access → allow all IPs (0.0.0.0/0)
4. Connect → copy the connection string
5. Paste as MONGODB_URI in Railway

Made by B0zin0
