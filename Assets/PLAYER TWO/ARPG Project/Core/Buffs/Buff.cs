using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [CreateAssetMenu(fileName = "New Buff", menuName = "PLAYER TWO/ARPG Project/Buff/Buff")]
    public class Buff : ScriptableObject
    {
        [Header("General Settings")]
        public Sprite icon;
        public float duration = 5f;
        public float cooldown = 0f;
        public bool isDebuff;

        [Header("Stat Modifiers")]
        public int strength;
        public int dexterity;
        public int vitality;
        public int energy;
        public int defense;
        public int magicResistance;
        public int fireResistance;
        public int waterResistance;
        public int iceResistance;
        public int earthResistance;
        public int airResistance;
        public int lightningResistance;
        public int shadowResistance;
        public int lightResistance;
        public int arcaneResistance;
    }
}