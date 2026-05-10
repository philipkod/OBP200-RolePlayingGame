namespace OBP200_RolePlayingGame;

public abstract class CharacterClass
{
    public abstract string Name { get; }

    public abstract int StartMaxHp { get; }
    public abstract int StartAttack { get; }
    public abstract int StartDefense { get; }
    public abstract int StartGold { get; }
    public abstract int StartPotions { get; }

    public abstract int LevelUpMaxHpBonus { get; }
    public abstract int LevelUpAttackBonus { get; }
    public abstract int LevelUpDefenseBonus { get; }

    public abstract int CalculateDamage(int attack, int enemyDefense, Random rng);

    public abstract int UseSpecial(Player player, int enemyDefense, bool vsBoss, Random rng);

    public virtual double RunAwayChance => 0.25;

    
    
    
}