namespace OBP200_RolePlayingGame;

public class Player
{
    public string Name { get; private set; }
    
    // Spelarens valda karaktärsklass
    public CharacterClass Class { get; private set; }
    
    
    public int MaxHp { get; private set; }
    public int Hp { get; private set; }
    public int Attack  { get; private set; }
    public int Defense { get; private set; }
    public int Xp { get; private set; }
    public int Gold { get; private set; }
    public int Level { get; private set; }
    public int Potions { get; private set; }
    
    public List<string> Inventory { get; private set; } = new();


    public Player(string name, CharacterClass characterClass)
    {
        Name = name;
        Class = characterClass;

        MaxHp = characterClass.StartMaxHp;
        Hp = characterClass.StartMaxHp;
        Attack = characterClass.StartAttack;
        Defense = characterClass.StartDefense;
        Gold = characterClass.StartGold;
        Potions = characterClass.StartPotions;

        Xp = 0;
        Level = 1;
        
        Inventory.Add("Wooden Sword");
        Inventory.Add("Cloth Armor");
    }

    public bool IsDead()
    {
        return Hp <= 0;
    }

    public void TakeDamage(int damage)
    {
        Hp = Math.Max(0, Hp - Math.Max(0, damage));
    }

    public void Heal(int amount)
    {
        Hp += amount;

        if (Hp > MaxHp)
        {
            Hp = MaxHp;
        }
    }

    public bool UsePotion()
    {
        if (Potions <= 0)
        {
            return false;
        }
        Potions--;
        Heal(12);
        return true;
    }

    public void AddGold(int amount)
    {
        Gold += Math.Max(0, amount);
    }

    public bool SpendGold(int amount)
    {
        if (Gold < amount)
        {
            return false;
        }
            
        Gold -= amount;
        return true;
            
    }

    public void AddXp(int amount)
    {
        Xp += Math.Max(0, amount);
        TryLevelUp();
    }

    public void IncreaseAttack(int amount)
    {
        Attack += Math.Max(0, amount);
    }

    public void IncreaseDefense(int amount)
    {
        Defense += Math.Max(0, amount);
    }

    public void AddPotion()
    {
        Potions++;
    }

    public void AddItem(string item)
    {
        Inventory.Add(item);
    }
    
    public int SellItems(string itemName, int pricePerItem)
    {
        int count = Inventory.Count(item => item == itemName);

        if (count == 0)
        {
            return 0;
        }

        Inventory.RemoveAll(item => item == itemName);
        AddGold(count * pricePerItem);

        return count;
    }

    private void TryLevelUp()
    {
        int nextThreshold = Level == 1 ? 10 : Level == 2 ? 25 : Level == 3 ? 45 : Level * 20;

        if (Xp < nextThreshold)
        {
            return;
        }

        Level++;

        MaxHp += Class.LevelUpMaxHpBonus;
        Attack += Class.LevelUpAttackBonus;
        Defense += Class.LevelUpDefenseBonus;

        Hp = MaxHp;

        Console.WriteLine("DU når nivå: " + Level);
    }

}