using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Game/Game Database")]
    public class GameDatabase : Singleton<GameDatabase>
    {
        [Tooltip("The Game Data object which references all the data from your game.")]
        public GameData gameData;

        public List<Character> characters => gameData.characters;
        public List<Item> items => gameData.items;
        public List<Skill> skills => gameData.skills;
        public List<Buff> buffs => gameData.buffs;
        public List<Quest> quests => gameData.quests;
        public List<GameObject> questItems => gameData.questItems;
        public List<GameObject> enemies => gameData.enemies;

        // === NEW: Specializations ===
        public List<Specializations> specializations => gameData.specializations;

        protected override void Awake()
        {
            base.Awake();

            SpecializationsRegisterAll();
        }

        private void SpecializationsRegisterAll()
        {
            if (specializations == null) return;

            foreach (var s in specializations)
            {
                if (s == null) continue;
                // wymuszamy OnEnable rejestrację
                if (Specializations.FindById(s.id) != s)
                {
                    // dodajemy ręcznie jeśli trzeba
                    var lookupField = typeof(Specializations)
                        .GetField("s_lookup", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                    var dict = lookupField?.GetValue(null) as Dictionary<int, Specializations>;
                    if (dict != null)
                        dict[s.id] = s;
                }
            }
        }

        public Specializations GetSpecializationById(int id) =>
            specializations.FirstOrDefault(s => s != null && s.id == id);

        public List<Specializations> GetSpecializationsByFamily(string family) =>
            specializations
                .Where(s => s != null && s.family == family)
                .OrderBy(s => s.tier)
                .ToList();

        public int GetElementId<T>(T element) where T : ScriptableObject
        {
            if (element is Character)
                return characters.IndexOf(element as Character);
            else if (element is Item)
                return items.IndexOf(element as Item);
            else if (element is Skill)
                return skills.IndexOf(element as Skill);
            else if (element is Buff)
                return buffs.IndexOf(element as Buff);
            else if (element is Quest)
                return quests.IndexOf(element as Quest);
            else if (element is Specializations)
                return specializations.IndexOf(element as Specializations);

            return -1;
        }

        public T FindElementById<T>(int id) where T : ScriptableObject
        {
            var type = typeof(T);

            if (type == typeof(Character) && !OutOfRangeFor<Character>(id, characters))
                return characters[id] as T;
            else if (type == typeof(Item) && !OutOfRangeFor<Item>(id, items))
                return items[id] as T;
            else if (type == typeof(Skill) && !OutOfRangeFor<Skill>(id, skills))
                return skills[id] as T;
            else if (type == typeof(Buff) && !OutOfRangeFor<Buff>(id, buffs))
                return buffs[id] as T;
            else if (type == typeof(Quest) && !OutOfRangeFor<Quest>(id, quests))
                return quests[id] as T;
            else if (type == typeof(Specializations) && !OutOfRangeFor<Specializations>(id, specializations))
                return specializations[id] as T;

            return default(T);
        }

        protected virtual bool OutOfRangeFor<T>(int index, List<T> list) =>
            index < 0 || index >= list.Count;
    }
}
