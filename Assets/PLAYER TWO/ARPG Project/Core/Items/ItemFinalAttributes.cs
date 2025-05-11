namespace PLAYERTWO.ARPGProject
{
    public struct ItemFinalAttributes
    {
        public int damage;
        public int magicDamage;
        public int magicResistance;
        public int attackSpeed;
        public int defense;
        public int mana;
        public int health;
        public float criticalChanceMultiplier;
        public float damageMultiplier;
        public float magicDamageMultiplier;
        public float defenseMultiplier;
        public float manaMultiplier;
        public float healthMultiplier;

        public ItemFinalAttributes(
    int damage = 0,
    int magicDamage = 0,
    int magicResistance = 0,
    int attackSpeed = 0,
    int defense = 0,
    int mana = 0,
    int health = 0,
    float criticalChanceMultiplier = 1f,
    float damageMultiplier = 1f,
    float magicDamageMultiplier = 1f,
    float defenseMultiplier = 1f,
    float manaMultiplier = 1f,
    float healthMultiplier = 1f)
    {
        this.damage = damage;
        this.magicDamage = magicDamage;
        this.magicResistance = magicResistance;
        this.attackSpeed = attackSpeed;
        this.defense = defense;
        this.mana = mana;
        this.health = health;
        this.criticalChanceMultiplier = criticalChanceMultiplier;
        this.damageMultiplier = damageMultiplier;
        this.magicDamageMultiplier = magicDamageMultiplier;
        this.defenseMultiplier = defenseMultiplier;
        this.manaMultiplier = manaMultiplier;
        this.healthMultiplier = healthMultiplier;
    }

        /// <summary>
        /// Combines the attributes of the items into a single struct.
        /// </summary>
        public ItemFinalAttributes(ItemInstance[] items)
        {
            damage = magicDamage = magicResistance = attackSpeed = defense = health = mana = 0;
            damageMultiplier = magicDamageMultiplier = criticalChanceMultiplier =
            defenseMultiplier = manaMultiplier = healthMultiplier = 1f;

            foreach (var item in items)
            {
                if (item == null)
                    continue;

                damage += item.GetAdditionalDamage();
                magicDamage += item.GetAdditionalMagicDamage();
                magicResistance += item.GetAdditionalMagicResistance();
                attackSpeed += item.GetAttackSpeed();
                defense += item.GetAdditionalDefense();
                mana += item.GetAdditionalMana();
                health += item.GetAdditionalHealth();
                damageMultiplier += item.GetDamageMultiplier();
                magicDamageMultiplier += item.GetMagicDamageMultiplier();
                criticalChanceMultiplier += item.GetCriticalChanceMultiplier();
                defenseMultiplier += item.GetDefenseMultiplier();
                manaMultiplier += item.GetManaMultiplier();
                healthMultiplier += item.GetHealthMultiplier();
            }
        }
    }
}
