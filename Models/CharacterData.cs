using System.Collections.Generic;

namespace JesseDex.Models
{
    public static class CharacterData
    {
        public static readonly List<Character> All = new()
        {
            new Character { Name = "Jesse",        Description = "The hero of Story Mode. Brave, resourceful, and always ready to help their friends.",                    ImageUrl = "https://static.wikia.nocookie.net/minecraft-story-mode/images/d/d0/Jesse.png",         Rarity = "Legendary", Season = "S1", SpawnWeight = 10  },
            new Character { Name = "Petra",        Description = "A skilled fighter and Jesse's closest ally. Tough as nails and fiercely loyal.",                          ImageUrl = "https://static.wikia.nocookie.net/minecraft-story-mode/images/8/8e/Petra.png",        Rarity = "Epic",      Season = "S1", SpawnWeight = 25  },
            new Character { Name = "Lukas",        Description = "A writer and former Ocelot member. Thoughtful and level-headed.",                                         ImageUrl = "https://static.wikia.nocookie.net/minecraft-story-mode/images/5/5f/Lukas.png",        Rarity = "Epic",      Season = "S1", SpawnWeight = 25  },
            new Character { Name = "Olivia",       Description = "A talented engineer and Jesse's best friend. Loves redstone and building.",                               ImageUrl = "https://static.wikia.nocookie.net/minecraft-story-mode/images/b/b5/Olivia.png",       Rarity = "Rare",      Season = "S1", SpawnWeight = 50  },
            new Character { Name = "Axel",         Description = "Big, loud, and surprisingly kind. Jesse's other best friend.",                                            ImageUrl = "https://static.wikia.nocookie.net/minecraft-story-mode/images/3/3e/Axel.png",         Rarity = "Rare",      Season = "S1", SpawnWeight = 50  },
            new Character { Name = "Reuben",       Description = "Jesse's beloved pet pig. Small but mighty.",                                                              ImageUrl = "https://static.wikia.nocookie.net/minecraft-story-mode/images/9/9e/Reuben.png",       Rarity = "Legendary", Season = "S1", SpawnWeight = 8   },
            new Character { Name = "Gabriel",      Description = "A legendary warrior and member of the Order of the Stone.",                                               ImageUrl = "https://static.wikia.nocookie.net/minecraft-story-mode/images/6/6c/Gabriel.png",      Rarity = "Epic",      Season = "S1", SpawnWeight = 20  },
            new Character { Name = "Ellegaard",    Description = "The redstone engineer of the Order of the Stone. Brilliant and a little arrogant.",                      ImageUrl = "https://static.wikia.nocookie.net/minecraft-story-mode/images/e/e1/Ellegaard.png",    Rarity = "Epic",      Season = "S1", SpawnWeight = 20  },
            new Character { Name = "Magnus",       Description = "The griefer of the Order of the Stone. Loud, chaotic, and kind of fun.",                                  ImageUrl = "https://static.wikia.nocookie.net/minecraft-story-mode/images/8/8b/Magnus.png",       Rarity = "Epic",      Season = "S1", SpawnWeight = 20  },
            new Character { Name = "Soren",        Description = "The builder of the Order of the Stone. A genius hiding at the end of the world.",                         ImageUrl = "https://static.wikia.nocookie.net/minecraft-story-mode/images/3/39/Soren.png",        Rarity = "Rare",      Season = "S1", SpawnWeight = 40  },
            new Character { Name = "Ivor",         Description = "The eccentric potion maker who started everything. Complicated but ultimately good.",                     ImageUrl = "https://static.wikia.nocookie.net/minecraft-story-mode/images/4/42/Ivor.png",         Rarity = "Rare",      Season = "S1", SpawnWeight = 40  },
            new Character { Name = "Cassie Rose",  Description = "The mysterious White Pumpkin. A master of disguise and traps.",                                           ImageUrl = "https://static.wikia.nocookie.net/minecraft-story-mode/images/c/c8/Cassie_Rose.png",  Rarity = "Legendary", Season = "S1", SpawnWeight = 5   },
            new Character { Name = "Radar",        Description = "Jesse's enthusiastic new assistant in Season 2. Clumsy but full of heart.",                               ImageUrl = "https://static.wikia.nocookie.net/minecraft-story-mode/images/7/74/Radar.png",        Rarity = "Rare",      Season = "S2", SpawnWeight = 50  },
            new Character { Name = "Jack",         Description = "A legendary explorer Jesse meets in Season 2. Tough and experienced.",                                    ImageUrl = "https://static.wikia.nocookie.net/minecraft-story-mode/images/5/52/Jack.png",         Rarity = "Epic",      Season = "S2", SpawnWeight = 25  },
            new Character { Name = "Nurm",         Description = "Jack's loyal villager companion. Doesn't say much but means everything.",                                 ImageUrl = "https://static.wikia.nocookie.net/minecraft-story-mode/images/4/45/Nurm.png",         Rarity = "Common",    Season = "S2", SpawnWeight = 80  },
            new Character { Name = "Harper",       Description = "A brilliant scientist and former Old Builder. Helps Jesse navigate the Admin's world.",                   ImageUrl = "https://static.wikia.nocookie.net/minecraft-story-mode/images/6/6e/Harper.png",       Rarity = "Epic",      Season = "S2", SpawnWeight = 20  },
            new Character { Name = "Stella",       Description = "Jesse's rival in Season 2. Rich, competitive, and surprisingly complex.",                                 ImageUrl = "https://static.wikia.nocookie.net/minecraft-story-mode/images/9/9b/Stella.png",       Rarity = "Rare",      Season = "S2", SpawnWeight = 45  },
            new Character { Name = "The Admin",    Description = "The all-powerful villain of Season 2. Controls the world itself.",                                        ImageUrl = "https://static.wikia.nocookie.net/minecraft-story-mode/images/3/35/Romeo.png",        Rarity = "Legendary", Season = "S2", SpawnWeight = 5   },
            new Character { Name = "Stampy",       Description = "The beloved Stampy Cat in block form. A special crossover character.",                                    ImageUrl = "https://static.wikia.nocookie.net/minecraft-story-mode/images/a/a1/Stampylonghead.png", Rarity = "Legendary", Season = "Special", SpawnWeight = 3 },
            new Character { Name = "LDShadowLady", Description = "Lizzie from the DanTDM crossover. A rare and special guest character.",                                  ImageUrl = "https://static.wikia.nocookie.net/minecraft-story-mode/images/6/6b/LDShadowLady.png", Rarity = "Legendary", Season = "Special", SpawnWeight = 3 },
        };

        public static Character GetWeightedRandom()
        {
            var total = 0;
            foreach (var c in All) total += c.SpawnWeight;

            var roll = new Random().Next(total);
            var sum  = 0;
            foreach (var c in All)
            {
                sum += c.SpawnWeight;
                if (roll < sum) return c;
            }
            return All[0];
        }

        public static string RarityColor(string rarity) => rarity switch
        {
            "Common"    => "#AAAAAA",
            "Rare"      => "#4488FF",
            "Epic"      => "#AA44FF",
            "Legendary" => "#FFAA00",
            _           => "#FFFFFF"
        };

        public static uint RarityColorUint(string rarity) => rarity switch
        {
            "Common"    => 0xAAAAAA,
            "Rare"      => 0x4488FF,
            "Epic"      => 0xAA44FF,
            "Legendary" => 0xFFAA00,
            _           => 0xFFFFFF
        };
    }
}
