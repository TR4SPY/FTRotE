using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    public interface ItemQuest
    {
        bool IsQuestSpecific { get; } // Wskazuje, czy przedmiot jest związany z questem
        //string QuestDescription { get; } // Opis questu
    }
}