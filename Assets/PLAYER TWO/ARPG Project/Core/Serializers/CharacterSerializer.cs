using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [System.Serializable]
    public class CharacterSerializer
    {
        public int characterId;

        public string name;
        public string scene;

        public UnitySerializer.Vector3 position;
        public UnitySerializer.Vector3 rotation;

        public StatsSerializer stats;
        public EquipmentsSerializer equipments;
        public InventorySerializer inventory;
        public SkillsSerializer skills;
        public QuestsSerializer quests;
        public ScenesSerializer scenes;

        // Dodane pola dla mnożników trudności
        public float dexterityMultiplier = 1.0f;
        public float strengthMultiplier = 1.0f;
        public float speedMultiplier = 1.0f;

        public CharacterSerializer(CharacterInstance character)
        {
            characterId = GameDatabase.instance.GetElementId<Character>(character.data);
            name = character.name;
            position = new UnitySerializer.Vector3(character.currentPosition);
            rotation = new UnitySerializer.Vector3(character.currentRotation.eulerAngles);
            scene = character.currentScene;
            stats = new StatsSerializer(character.stats);
            equipments = new EquipmentsSerializer(character.equipments);
            inventory = new InventorySerializer(character.inventory);
            skills = new SkillsSerializer(character.skills);
            quests = new QuestsSerializer(character.quests);
            scenes = new ScenesSerializer(character.scenes);

            // Zapisywanie mnożników z CharacterInstance
            dexterityMultiplier = character.GetMultiplier("Dexterity");
            strengthMultiplier = character.GetMultiplier("Strength");
            speedMultiplier = character.GetMultiplier("Speed");
        }

        public virtual string ToJson() => JsonUtility.ToJson(this);

        public static CharacterSerializer FromJson(string json) =>
            JsonUtility.FromJson<CharacterSerializer>(json);
    }
}
