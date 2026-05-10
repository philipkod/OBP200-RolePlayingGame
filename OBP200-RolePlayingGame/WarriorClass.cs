namespace OBP200_RolePlayingGame;

public class WarriorClass : CharacterClass
{
    public override string Name => "Warrior";

    public override int StartMaxHp => 40;
    public override int StartAttack => 7;
    public override int StartDefense => 5;
    public override int StartGold => 15;
    public override int StartPotions => 2;
    
    public override int LevelUpMaxHpBonus => 6;
    public override int LevelUpAttackBonus => 2;
    public override int LevelUpDefenseBonus => 2;

    public override int CalculateDamage(int attack, int enemyDefense, Random rng)
    {
        int baseDamage = Math.Max(1, attack - enemyDefense / 2);
        int roll = rng.Next(0, 3);

        return Math.Max(1, baseDamage + 1 + roll);
    }

    public override int UseSpecial(Player player, int enemyDefense, bool vsBoss, Random rng)
    {
        Console.WriteLine("Warrior använder Heavy Strike!");

        int damage = Math.Max(2, player.Attack + 3 - enemyDefense);

        player.TakeDamage(2);

        if (vsBoss)
        {
            damage = (int)Math.Round(damage * 0.8);
        }

        return Math.Max(0, damage);
    }
}

