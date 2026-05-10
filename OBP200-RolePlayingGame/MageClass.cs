namespace OBP200_RolePlayingGame;

public class MageClass : CharacterClass
{
    public override string Name => "Mage";

    public override int StartMaxHp => 28;
    public override int StartAttack => 10;
    public override int StartDefense => 2;
    public override int StartGold => 15;
    public override int StartPotions => 2;

    public override int LevelUpMaxHpBonus => 4;
    public override int LevelUpAttackBonus => 4;
    public override int LevelUpDefenseBonus => 1;

    public override double RunAwayChance => 0.35;
    
    
    public override int CalculateDamage(int attack, int enemyDefense, Random rng)
    {
        int baseDamage = Math.Max(1, attack - enemyDefense / 2);
        int roll = rng.Next(0, 3);

        return Math.Max(1, baseDamage + 2 + roll);
    }

    public override int UseSpecial(Player player, int enemyDefense, bool vsBoss, Random rng)
    {
        if (!player.SpendGold(3))
        {
            Console.WriteLine("Inte tillräckligt med guld för att kasta Fireball (kostar 3).");
            return 0;
        }

        Console.WriteLine("Mage kastar Fireball!");

        int damage = Math.Max(3, player.Attack + 5 - enemyDefense / 2);

        if (vsBoss)
        {
            damage = (int)Math.Round(damage * 0.8);
        }

        return Math.Max(0, damage);
    }

    
}