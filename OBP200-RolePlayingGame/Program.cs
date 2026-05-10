using System.Text;

namespace OBP200_RolePlayingGame;


class Program
{
    // ======= Globalt tillstånd  =======
    
    private static Player Player;

    // Rum: [type, label]
    // types: battle, treasure, shop, rest, boss
    static List<string[]> Rooms = new List<string[]>();

    // Fiendemallar: [type, name, HP, ATK, DEF, XPReward, GoldReward]
    static List<string[]> EnemyTemplates = new List<string[]>();

    // Status för kartan
    static int CurrentRoomIndex = 0;

    // Random Rng
    static Random Rng = new Random();

    // ======= Main =======

    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        InitEnemyTemplates();

        while (true)
        {
            ShowMainMenu();
            Console.Write("Välj: ");
            var choice = (Console.ReadLine() ?? "").Trim();

            if (choice == "1")
            {
                StartNewGame();
                RunGameLoop();
            }
            else if (choice == "2")
            {
                Console.WriteLine("Avslutar...");
                return;
            }
            else
            {
                Console.WriteLine("Ogiltigt val.");
            }

            Console.WriteLine();
        }
    }

    // ======= Meny & Init =======

    static void ShowMainMenu()
    {
        Console.WriteLine("=== Text-RPG ===");
        Console.WriteLine("1. Nytt spel");
        Console.WriteLine("2. Avsluta");
    }

    static void StartNewGame()
    {
        Console.Write("Ange namn: ");
        var name = (Console.ReadLine() ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name)) name = "Namnlös";

        Console.WriteLine("Välj klass: 1) Warrior  2) Mage  3) Rogue");
        Console.Write("Val: ");
        var k = (Console.ReadLine() ?? "").Trim();

        
        CharacterClass characterClass;
        
        switch (k)
        {
            case "1":
                characterClass = new WarriorClass();
                break;

            case "2":
                characterClass = new MageClass();
                break;

            case "3":
                characterClass = new RogueClass();
                break;

            default:
                characterClass = new WarriorClass();
                break;
        }
        
        Player = new Player(name, characterClass);
        

        // Initiera karta (linjärt äventyr)
        Rooms.Clear();
        Rooms.Add(new[] { "battle", "Skogsstig" });
        Rooms.Add(new[] { "treasure", "Gammal kista" });
        Rooms.Add(new[] { "shop", "Vandrande köpman" });
        Rooms.Add(new[] { "battle", "Grottans mynning" });
        Rooms.Add(new[] { "rest", "Lägereld" });
        Rooms.Add(new[] { "battle", "Grottans djup" });
        Rooms.Add(new[] { "boss", "Urdraken" });

        CurrentRoomIndex = 0;

        Console.WriteLine($"Välkommen, {Player.Name} the {Player.Class.Name}!");
        ShowStatus();
    }

    static void RunGameLoop()
    {
        while (true)
        {
            var room = Rooms[CurrentRoomIndex];
            Console.WriteLine($"--- Rum {CurrentRoomIndex + 1}/{Rooms.Count}: {room[1]} ({room[0]}) ---");

            bool continueAdventure = EnterRoom(room[0]);
            
            if (IsPlayerDead())
            {
                Console.WriteLine("Du har stupat... Spelet över.");
                break;
            }
            
            if (!continueAdventure)
            {
                Console.WriteLine("Du lämnar äventyret för nu.");
                break;
            }

            CurrentRoomIndex++;
            
            if (CurrentRoomIndex >= Rooms.Count)
            {
                Console.WriteLine();
                Console.WriteLine("Du har klarat äventyret!");
                break;
            }
            
            Console.WriteLine();
            Console.WriteLine("[C] Fortsätt     [Q] Avsluta till huvudmeny");
            Console.Write("Val: ");
            var post = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

            if (post == "Q")
            {
                Console.WriteLine("Tillbaka till huvudmenyn.");
                break;
            }

            Console.WriteLine();
        }
    }

    // ======= Rumshantering =======

    static bool EnterRoom(string type)
    {
        switch ((type ?? "battle").Trim())
        {
            case "battle":
                return DoBattle(isBoss: false);
            case "boss":
                return DoBattle(isBoss: true);
            case "treasure":
                return DoTreasure();
            case "shop":
                return DoShop();
            case "rest":
                return DoRest();
            default:
                Console.WriteLine("Du vandrar vidare...");
                return true;
        }
    }

    // ======= Strid =======

    static bool DoBattle(bool isBoss)
    {
        var enemy = GenerateEnemy(isBoss);
        Console.WriteLine($"En {enemy[1]} dyker upp! (HP {enemy[2]}, ATK {enemy[3]}, DEF {enemy[4]})");

        int enemyHp = ParseInt(enemy[2], 10);
        int enemyAtk = ParseInt(enemy[3], 3);
        int enemyDef = ParseInt(enemy[4], 0);

        while (enemyHp > 0 && !IsPlayerDead())
        {
            Console.WriteLine();
            ShowStatus();
            Console.WriteLine($"Fiende: {enemy[1]} HP={enemyHp}");
            Console.WriteLine("[A] Attack   [X] Special   [P] Dryck   [R] Fly");
            if (isBoss) Console.WriteLine("(Du kan inte fly från en boss!)");
            Console.Write("Val: ");

            var cmd = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

            if (cmd == "A")
            {
                int damage = CalculatePlayerDamage(enemyDef);
                enemyHp -= damage;
                Console.WriteLine($"Du slog {enemy[1]} för {damage} skada.");
            }
            else if (cmd == "X")
            {
                int special = UseClassSpecial(enemyDef, isBoss);
                enemyHp -= special;
                Console.WriteLine($"Special! {enemy[1]} tar {special} skada.");
            }
            else if (cmd == "P")
            {
                UsePotion();
            }
            else if (cmd == "R" && !isBoss)
            {
                if (TryRunAway())
                {
                    Console.WriteLine("Du flydde!");
                    return true; // fortsätt äventyr
                }
                else
                {
                    Console.WriteLine("Misslyckad flykt!");
                }
            }
            else
            {
                Console.WriteLine("Du tvekar...");
            }

            if (enemyHp <= 0) break;

            // Fiendens tur
            int enemyDamage = CalculateEnemyDamage(enemyAtk);
            ApplyDamageToPlayer(enemyDamage);
            Console.WriteLine($"{enemy[1]} anfaller och gör {enemyDamage} skada!");
        }

        if (IsPlayerDead())
        {
            return false; // avsluta äventyr
        }

        // Vinstrapporter, XP, guld, loot
        int xpReward = ParseInt(enemy[5], 5);
        int goldReward = ParseInt(enemy[6], 3);

        AddPlayerXp(xpReward);
        AddPlayerGold(goldReward);

        Console.WriteLine($"Seger! +{xpReward} XP, +{goldReward} guld.");
        MaybeDropLoot(enemy[1]);

        return true;
    }

    static string[] GenerateEnemy(bool isBoss)
    {
        if (isBoss)
        {
            // Boss-mall
            return new[] { "boss", "Urdraken", "55", "9", "4", "30", "50" };
        }
        else
        {
            // Slumpa bland templates
            var template = EnemyTemplates[Rng.Next(EnemyTemplates.Count)];
            
            // Slmumpmässig justering av stats
            int hp = ParseInt(template[2], 10) + Rng.Next(-1, 3);
            int atk = ParseInt(template[3], 3) + Rng.Next(0, 2);
            int def = ParseInt(template[4], 0) + Rng.Next(0, 2);
            int xp = ParseInt(template[5], 4) + Rng.Next(0, 3);
            int gold = ParseInt(template[6], 2) + Rng.Next(0, 3);
            return new[] { template[0], template[1], hp.ToString(), atk.ToString(), def.ToString(), xp.ToString(), gold.ToString() };
        }
    }

    static void InitEnemyTemplates()
    {
        EnemyTemplates.Clear();
        EnemyTemplates.Add(new[] { "beast", "Vildsvin", "18", "4", "1", "6", "4" });
        EnemyTemplates.Add(new[] { "undead", "Skelett", "20", "5", "2", "7", "5" });
        EnemyTemplates.Add(new[] { "bandit", "Bandit", "16", "6", "1", "8", "6" });
        EnemyTemplates.Add(new[] { "slime", "Geléslem", "14", "3", "0", "5", "3" });
    }

    static int CalculatePlayerDamage(int enemyDef)
    {
        return Player.Class.CalculateDamage(Player.Attack, enemyDef, Rng);
    }
    
    static int UseClassSpecial(int enemyDef, bool vsBoss)
    {
        return Player.Class.UseSpecial(Player, enemyDef, vsBoss, Rng);
    }

    static int CalculateEnemyDamage(int enemyAtk)
    {
        int roll = Rng.Next(0, 3);
        int dmg = Math.Max(1, enemyAtk - Player.Defense / 2) + roll;

        // Liten chans till "glancing blow" (minskad skada)
        if (Rng.NextDouble() < 0.1)
        {
            dmg = Math.Max(1, dmg - 2);
        }

        return dmg;
    }

    static void ApplyDamageToPlayer(int dmg)
    {
        Player.TakeDamage(dmg);
    }

    static void UsePotion()
    {
        int oldHp = Player.Hp;
        
        if (!Player.UsePotion())
        {
            Console.WriteLine("Du har inga drycker kvar.");
            return;
        }
        
        Console.WriteLine($"Du dricker en dryck och återfår {Player.Hp - oldHp} HP.");
    }

    static bool TryRunAway()
    {
        return Rng.NextDouble() < Player.Class.RunAwayChance;
    }

    static bool IsPlayerDead()
    {
        return Player.IsDead();
    }

    static void AddPlayerXp(int amount)
    {
        Player.AddXp(amount);
    }

    static void AddPlayerGold(int amount)
    {
        Player.AddGold(amount);
    }
    
    static void MaybeDropLoot(string enemyName)
    {
        // Enkel loot-regel
        if (Rng.NextDouble() < 0.35)
        {
            string item = "Minor Gem";

            if (enemyName.Contains("Urdraken"))
            {
                item = "Dragon Scale";
            }

            Player.AddItem(item);
            
            Console.WriteLine($"Föremål hittat: {item} (lagt i din väska)");
        }
    }

    // ======= Rumshändelser =======

    static bool DoTreasure()
    {
        Console.WriteLine("Du hittar en gammal kista...");
        if (Rng.NextDouble() < 0.5)
        {
            int gold = Rng.Next(8, 15);
            AddPlayerGold(gold);
            Console.WriteLine($"Kistan innehåller {gold} guld!");
        }
        else
        {
            var items = new[] { "Iron Dagger", "Oak Staff", "Leather Vest", "Healing Herb" };
            string found = items[Rng.Next(items.Length)];
            
            Player.AddItem(found);
            
            Console.WriteLine($"Du plockar upp: {found}");
        }
        return true;
    }

    static bool DoShop()
    {
        Console.WriteLine("En vandrande köpman erbjuder sina varor:");
        while (true)
        {
            Console.WriteLine($"Guld: {Player.Gold} | Drycker: {Player.Potions}");
            Console.WriteLine("1) Köp dryck (10 guld)");
            Console.WriteLine("2) Köp vapen (+2 ATK) (25 guld)");
            Console.WriteLine("3) Köp rustning (+2 DEF) (25 guld)");
            Console.WriteLine("4) Sälj alla 'Minor Gem' (+5 guld/st)");
            Console.WriteLine("5) Lämna butiken");
            Console.Write("Val: ");
            var val = (Console.ReadLine() ?? "").Trim();

            if (val == "1")
            {
                TryBuy(10, () => Player.AddPotion(), "Du köper en dryck.");
            }
            else if (val == "2")
            {
                TryBuy(25, () => Player.IncreaseAttack(2), "Du köper ett bättre vapen.");
            }
            else if (val == "3")
            {
                TryBuy(25, () => Player.IncreaseDefense(2), "Du köper bättre rustning.");
            }
            else if (val == "4")
            {
                SellMinorGems();
            }
            else if (val == "5")
            {
                Console.WriteLine("Du säger adjö till köpmannen.");
                break;
            }
            else
            {
                Console.WriteLine("Köpmannen förstår inte ditt val.");
            }
        }
        return true;
    }

    static void TryBuy(int cost, Action apply, string successMsg)
    {
        if (Player.SpendGold(cost))
        {
            apply();
            Console.WriteLine(successMsg);
        }
        else
        {
            Console.WriteLine("Du har inte råd.");
        }
    }

    static void SellMinorGems()
    {
        int soldCount = Player.SellItems("Minor Gem", 5);

        if (soldCount == 0)
        {
            Console.WriteLine("Inga 'Minor Gem' i väskan.");
            return;
        }

        Console.WriteLine($"Du säljer {soldCount} st Minor Gem för {soldCount * 5} guld.");

    }

    static bool DoRest()
    {
        Console.WriteLine("Du slår läger och vilar.");
        
        Player.Heal(Player.MaxHp);
        
        Console.WriteLine("HP återställt till max.");
        
        return true;
    }

    // ======= Status =======

    static void ShowStatus()
    {
        Console.WriteLine($"[{Player.Name} | {Player.Class.Name}] HP {Player.Hp}/{Player.MaxHp} ATK {Player.Attack} DEF {Player.Defense} LVL {Player.Level} XP {Player.Xp} Guld {Player.Gold} Drycker {Player.Potions}");

        if (Player.Inventory.Count > 0) 
        {
            Console.WriteLine($"Väska: {string.Join(";", Player.Inventory)}");
        }
    }
    
    // ======= Hjälpmetoder =======

    static int ParseInt(string s, int fallback)
    {
        try
        {
            int value = Convert.ToInt32(s);
            return value;
        }
        catch (Exception e)
        {
            return fallback;
        }
    }
}
