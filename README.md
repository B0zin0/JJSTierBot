# JJS Tier Bot

Discord bot for managing the JJS Madness Tournament tier list.

## Commands

| Command | Description |
|---|---|
| `/setup` | Set tier list and log channels (run this first) |
| `/add` | Add a player |
| `/remove` | Remove a player |
| `/retire` | Retire a player |
| `/rank` | Change a player's rank |
| `/list` | Show the full tier list |
| `/challenge` | Log a challenge result |
| `/history` | Show recent changes |

## Ranks
S → A+ → A → B → C → D → F → Unranked

## Rules
- To reach A+ you must beat the two lowest A+ players
- S and A+ require beating everyone below you in that tier
- B tier and below can skip ahead

## Deploy on Railway
1. Push to GitHub
2. New project on railway.app → connect repo
3. Add variable: `DISCORD_TOKEN`
4. Deploy
5. Run `/setup` in your server to configure channels
