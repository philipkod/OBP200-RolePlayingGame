namespace OBP200_RolePlayingGame;

public class RogueClass : CharacterClass
{
    public override string Name => "Rogue";

    public override int StartMaxHp => 32;
    public override int StartAttack => 8;
    public override int StartDefense => 3;
    public override int StartGold => 20;
    public override int StartPotions => 3;

    public override int LevelUpMaxHpBonus => 5;
    public override int LevelUpAttackBonus => 3;
    public override int LevelUpDefenseBonus => 1;

    public override double RunAwayChance => 0.5;

    public override int CalculateDamage(int attack, int enemyDefense, Random rng)
    {
        int baseDamage = Math.Max(1, attack - enemyDefense / 2);
        int roll = rng.Next(0, 3);

        if (rng.NextDouble() < 0.2)
        {
            baseDamage += 4;
        }

        return Math.Max(1, baseDamage + roll);
    }

    public override int UseSpecial(Player player, int enemyDefense, bool vsBoss, Random rng)
    {
        int damage;

        if (rng.NextDouble() < 0.5)
        {
            Console.WriteLine("Rogue utför en lyckad Backstab!");
            damage = Math.Max(4, player.Attack + 6);
        }
        else
        {
            Console.WriteLine("Backstab misslyckades!");
            damage = 1;
        }

        if (vsBoss)
        {
            damage = (int)Math.Round(damage * 0.8);
        }

        return Math.Max(0, damage);
    }
}