using System;
using UnityEngine;
using System.Collections.Generic;
using TMPro;

namespace PLAYERTWO.ARPGProject
{
    public class CharacterInstance
    {
        public Character data;
        public string name;
        public string currentScene;
        
        public Vector3 initialPosition;
        public Quaternion initialRotation;

        public CharacterStats stats;

        private Dictionary<string, float> multipliers = new Dictionary<string, float>
        {
            { "Dexterity", 1.0f },
            { "Strength", 1.0f },
            { "Speed", 1.0f }
        };

        public HashSet<string> visitedZones = new HashSet<string>();
        public HashSet<int> activatedWaypoints = new HashSet<int>();
        public Dictionary<string, HashSet<int>> viewedDialogPages = new Dictionary<string, HashSet<int>>();

        public Dictionary<int, int> selectedDialogPaths = new Dictionary<int, int>();

        public List<string> unlockedAchievements = new List<string>();

        public CharacterEquipments equipments;
        public CharacterInventory inventory;
        public CharacterSkills skills;
        public CharacterQuests quests;
        public CharacterScenes scenes;
        public BuffsSerializer buffs;

        public CharacterSpecializations specializations = new CharacterSpecializations();
        public event Action onBuffsRestored;

        public int savedHealth = -1;
        public int savedMana = -1;
        public float savedDifficulty = 5f; 
        public int playerDeaths = 0;
        public int enemiesDefeated = 0;
        public float totalCombatTime = 0f;
        public int potionsUsed = 0;
        public int difficultyMultiplier = 0;
        public int zonesDiscovered = 0;
        public int npcInteractions = 0;
        public int questsCompleted = 0;
        public int waypointsDiscovered = 0;
        public int achievementsUnlocked = 0;
        public bool questionnaireCompleted = false;
        public bool storylineCompleted = false;

        public string playerType = "Undefined";
        public string currentDynamicPlayerType = "Unknown";

        public string guildName = "";
        public string guildCrestData = "";
        [System.NonSerialized] public Sprite guildCrest;
        [System.NonSerialized] public TMP_SpriteAsset guildCrestTMPAsset;

        public float totalPlayTime = 0f;
        protected Entity m_entity;
        public Entity Entity => m_entity;
        public Affinity affinity = Affinity.None;

        public Vector3 currentPosition => m_entity ? m_entity.position : initialPosition;
        public Quaternion currentRotation => m_entity ? m_entity.transform.rotation : initialRotation;
        public bool HasViewedDialogPage(string npcID, int pageIndex)
        {
            return viewedDialogPages.ContainsKey(npcID) && viewedDialogPages[npcID].Contains(pageIndex);
        }
        
        public CharacterInstance() { }

        public CharacterInstance(Character data, string name)
        {
            this.data = data;
            this.name = name;
            currentScene = data.initialScene;
            stats = new CharacterStats(data);
            equipments = new CharacterEquipments(data);
            inventory = new CharacterInventory(data);
            skills = new CharacterSkills(data);
            specializations = new CharacterSpecializations();
            quests = new CharacterQuests();
            scenes = new CharacterScenes();
            
        }

        public Dictionary<string, GameObject> GetEquippedPrefabs()
        {
            var equippedPrefabs = new Dictionary<string, GameObject>();

            if (equipments.currentRightHand?.data?.prefab != null)
            {
                equippedPrefabs.Add("RightHand", equipments.currentRightHand.data.prefab);
                Debug.Log($"RightHand prefab: {equipments.currentRightHand.data.prefab.name}");
                
            }

            if (equipments.currentLeftHand?.data?.prefab != null)
            {
                equippedPrefabs.Add("LeftHand", equipments.currentLeftHand.data.prefab);
                Debug.Log($"LeftHand prefab: {equipments.currentLeftHand.data.prefab.name}");
            }

            if (equipments.currentHelm?.data?.prefab != null)
            {
                equippedPrefabs.Add("Head", equipments.currentHelm.data.prefab);
                Debug.Log($"Head prefab: {equipments.currentHelm.data.prefab.name}");
            }

            if (equipments.currentChest?.data?.prefab != null)
            {
                equippedPrefabs.Add("Chest", equipments.currentChest.data.prefab);
                Debug.Log($"Chest prefab: {equipments.currentChest.data.prefab.name}");
            }

            if (equipments.currentPants?.data?.prefab != null)
            {
                equippedPrefabs.Add("Pants", equipments.currentPants.data.prefab);
                Debug.Log($"Pants prefab: {equipments.currentPants.data.prefab.name}");
            }

            if (equipments.currentGloves?.data?.prefab != null)
            {
                equippedPrefabs.Add("Gloves", equipments.currentGloves.data.prefab);
                Debug.Log($"Gloves prefab: {equipments.currentGloves.data.prefab.name}");
            }

            if (equipments.currentBoots?.data?.prefab != null)
            {
                equippedPrefabs.Add("Boots", equipments.currentBoots.data.prefab);
                Debug.Log($"Boots prefab: {equipments.currentBoots.data.prefab.name}");
            }

            return equippedPrefabs;
        }

        public void MarkDialogPageAsViewed(string npcID, int pageIndex)
        {
            if (!viewedDialogPages.ContainsKey(npcID))
            {
                viewedDialogPages[npcID] = new HashSet<int>();
            }
            
            if (!viewedDialogPages[npcID].Contains(pageIndex))
            {
                viewedDialogPages[npcID].Add(pageIndex);
                Debug.Log($"Dialog Page {pageIndex} viewed for NPC {npcID}");
            }
        }

        public void SetDialogPathChoice(int fromPage, int toPage)
        {
            selectedDialogPaths[fromPage] = toPage;
        }

        public int GetDialogNextPage(int currentPage)
        {
            return selectedDialogPaths.ContainsKey(currentPage) ? selectedDialogPaths[currentPage] : -1;
        }

        public void SelectSpecialization(int tier, Specializations def)
        {
            specializations.SelectSpecialization(tier, def);
        }

        public Specializations GetSelected(int tier)
        {
            return specializations.GetSelected(tier);
        }

        public void ResetAll()
        {
            specializations.ResetAll();
        }

        public string GetSpecialConditionAsString()
        {
            return specialCondition.ToString();
        }

        public Affinity specialCondition = Affinity.None;

            public void SetSpecialCondition(Affinity condition)
            {
                specialCondition = condition;
            }

        public static CharacterClassRestrictions GetClassBitFromName(string rawName)
        {
            string className = rawName.Replace("(Clone)", "").Trim();

            if (ClassHierarchy.NameToBits.TryGetValue(className, out var bit))
            {
                return bit;
            }
            else
            {
                return CharacterClassRestrictions.None;
            }
        }

        public string GetName()
        {
            if (Entity == null)
            {
                Debug.LogWarning("[CharacterInstance] GetName() called on destroyed Entity.");
                return "Unknown";
            }

            var cleanName = Entity.name?.Replace("(Clone)", "").Trim() ?? "";

            if (!ClassHierarchy.NameToBits.TryGetValue(cleanName, out var classType))
                return "Unknown";

            int tier = ClassHierarchy.GetTier(classType);

            if (specialCondition == Affinity.None)
                return classType.ToString();

            if (AffinityNaming.TierNames.TryGetValue(specialCondition, out var names) && tier < names.Length)
                return $"{names[tier]} {classType}";

            return classType.ToString();
        }

        /// <summary>
        /// Pobierz wartość mnożnika dla danej statystyki.
        /// </summary>
        public float GetMultiplier(string statName)
        {
            if (multipliers.ContainsKey(statName))
            {
                return multipliers[statName];
            }
            return 1.0f;
        }

        /// <summary>
        /// Ustaw wartość mnożnika dla danej statystyki.
        /// </summary>
        public void SetMultiplier(string statName, float value)
        {
            if (multipliers.ContainsKey(statName))
            {
                multipliers[statName] = value;
            }
            else
            {
                multipliers.Add(statName, value);
            }
        }

        public bool HasActivatedWaypoint(int waypointID) => activatedWaypoints.Contains(waypointID);

        public void MarkWaypointAsActivated(int waypointID)
        {
            if (!activatedWaypoints.Contains(waypointID))
            {
                activatedWaypoints.Add(waypointID);
                waypointsDiscovered++;
                Debug.Log($"Waypoint '{waypointID}' marked as visited for character '{name}'.");
            }
        }

        public bool HasVisitedZone(string zoneId) => visitedZones.Contains(zoneId);

        public void MarkZoneAsVisited(string zoneId)
        {
            if (!visitedZones.Contains(zoneId))
            {
                visitedZones.Add(zoneId);
                Debug.Log($"Zone '{zoneId}' marked as visited for character '{name}'.");
            }
        }

        /// <summary>
        /// Instantiates a new Entity from this Character Instance data.
        /// </summary>
        public virtual Entity Instantiate()
        {
            if (m_entity == null)
            {
                m_entity = GameObject.Instantiate(data.entity);
                stats.InitializeStats(m_entity.stats);
                equipments.InitializeEquipments(m_entity.items);
                inventory.InitializeInventory(m_entity.inventory);
                skills.InitializeSkills(m_entity.skills);
                RestoreWeaponSkill(m_entity);
                quests.InitializeQuests();
                scenes.InitializeScenes();
                buffs?.ApplyTo(m_entity.GetComponent<EntityBuffManager>());
                onBuffsRestored?.Invoke();
            }

            return m_entity;
        }

        public void RestoreSavedVitals()
        {
            if (m_entity == null) return;

            int hp = savedHealth < 0 ? m_entity.stats.maxHealth : savedHealth;
            int mp = savedMana   < 0 ? m_entity.stats.maxMana   : savedMana;

            m_entity.stats.health = Mathf.Clamp(hp, 0, m_entity.stats.maxHealth);
            m_entity.stats.mana   = Mathf.Clamp(mp, 0, m_entity.stats.maxMana);
        }

        void RestoreWeaponSkill(Entity entity)
        {
            var items = entity.items;
            var right = items.GetRightHand();
            var left = items.GetLeftHand();

            foreach (var item in new[] { right, left })
            {
                if (item?.data is ItemWeapon weapon)
                {
                    var skill = weapon.skill;
                    var source = weapon.skillSource;

                    if (item.isSkillEnabled && skill != null && source != null)
                    {
                        entity.skills.TryLearnSkill(source);
                    }
                }
            }
        }

        public void SetEntity(Entity entity)
        {
            m_entity = entity;
            Level.instance.SetPlayer(entity);
            EntityCamera.Instance?.SetTarget(entity);
            MinimapHUD.instance?.SetTarget(entity);
            Waypoint.SetPlayer(entity); 
        }


        public static CharacterInstance CreateFromSerializer(CharacterSerializer serializer)
        {
            var data = GameDatabase.instance.FindElementById<Character>(serializer.characterId);

            var characterInstance = new CharacterInstance()
            {
                data = data,
                name = serializer.name,
                currentScene = serializer.scene,
                initialPosition = serializer.position.ToUnity(),
                initialRotation = Quaternion.Euler(serializer.rotation.ToUnity()),
                stats = CharacterStats.CreateFromSerializer(serializer.stats),
                equipments = CharacterEquipments.CreateFromSerializer(serializer.equipments),
                inventory = CharacterInventory.CreateFromSerializer(serializer.inventory),
                skills = CharacterSkills.CreateFromSerializer(serializer.skills),
                specializations = CharacterSpecializations.CreateFromData(serializer.selectedSpecializations, serializer.specializationSkillPoints),
                quests = CharacterQuests.CreateFromSerializer(serializer.quests),
                scenes = CharacterScenes.CreateFromSerializer(serializer.scenes),
                
                buffs = serializer.buffs,

                playerDeaths = serializer.playerDeaths,
                enemiesDefeated = serializer.enemiesDefeated,
                totalCombatTime = serializer.totalCombatTime,
                npcInteractions = serializer.npcInteractions,
                questsCompleted = serializer.questsCompleted,
                waypointsDiscovered = serializer.waypointsDiscovered,
                questionnaireCompleted = serializer.questionnaireCompleted,
                storylineCompleted = serializer.storylineCompleted,
                playerType = serializer.playerType,
                currentDynamicPlayerType = serializer.currentDynamicPlayerType,
                totalPlayTime = serializer.totalPlayTime,

                viewedDialogPages = new Dictionary<string, HashSet<int>>(),
                visitedZones = serializer.visitedZones != null
                    ? new HashSet<string>(serializer.visitedZones)
                    : new HashSet<string>(),
                activatedWaypoints = serializer.activatedWaypoints != null
                    ? new HashSet<int>(serializer.activatedWaypoints)
                    : new HashSet<int>(),

                unlockedAchievements = serializer.unlockedAchievements != null
                    ? new List<string>(serializer.unlockedAchievements)
                    : new List<string>(),

                achievementsUnlocked = serializer.unlockedAchievements != null
                    ? serializer.unlockedAchievements.Count
                    : 0
            };

            if (serializer.viewedDialogPages != null)
            {
                characterInstance.viewedDialogPages = new Dictionary<string, HashSet<int>>();
                foreach (var entry in serializer.viewedDialogPages)
                {
                    characterInstance.viewedDialogPages[entry.Key] = new HashSet<int>(entry.Value);
                }
            }

            if (!Enum.TryParse(serializer.specialCondition, out Affinity parsedCondition))
            {
                Debug.LogError($"[AI-DDA] Błąd wczytywania specialCondition: {serializer.specialCondition}. Ustawiono 'None'.");
                parsedCondition = Affinity.None;
            }
            characterInstance.specialCondition = parsedCondition;

            characterInstance.savedHealth = serializer.health;
            characterInstance.savedMana = serializer.mana;

            characterInstance.SetMultiplier("Dexterity", serializer.dexterityMultiplier);
            characterInstance.SetMultiplier("Strength", serializer.strengthMultiplier);
            characterInstance.SetMultiplier("Vitality", serializer.vitalityMultiplier);
            characterInstance.SetMultiplier("Energy", serializer.energyMultiplier);
            characterInstance.savedDifficulty = serializer.savedDifficulty;
            characterInstance.guildName = serializer.guildName;
            characterInstance.guildCrestData = serializer.guildCrestData;
            characterInstance.guildCrest = DecodeSprite(serializer.guildCrestData);

            return characterInstance;
        }

        public void SetGuild(string name, Sprite crest, TMP_SpriteAsset crestAsset = null)
        {
            guildName = name;
            guildCrest = crest;
            guildCrestData = EncodeSprite(crest);
            guildCrestTMPAsset = crestAsset ?? CreateTMPAssetFromSprite(crest);
        }

        public Sprite GetGuildCrest()
        {
            if (guildCrest == null && !string.IsNullOrEmpty(guildCrestData))
                guildCrest = DecodeSprite(guildCrestData);
            return guildCrest;
        }

        public TMP_SpriteAsset GetTMPGuildCrestAsset()
        {
            if (guildCrestTMPAsset == null)
                guildCrestTMPAsset = CreateTMPAssetFromSprite(GetGuildCrest());
            return guildCrestTMPAsset;
        }

        private static TMP_SpriteAsset CreateTMPAssetFromSprite(Sprite sprite)
        {
            if (sprite == null) return null;

            var asset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
            asset.name = $"GuildTMPAsset_{sprite.name}";

            var spriteInfo = new TMP_Sprite
            {
                name = sprite.name,
                sprite = sprite,
                unicode = 0xE000
            };

            asset.spriteInfoList = new System.Collections.Generic.List<TMP_Sprite> { spriteInfo };
            asset.UpdateLookupTables();
            return asset;
        }

        private static string EncodeSprite(Sprite sprite)
        {

            if (sprite == null || sprite.texture == null)
                return string.Empty;
            try
            {
                return System.Convert.ToBase64String(sprite.texture.EncodeToPNG());
            }
            catch
            {
                return string.Empty;
            }
        }

        private static Sprite DecodeSprite(string data)
        {
            if (string.IsNullOrEmpty(data))
                return null;
            try
            {
                var bytes = System.Convert.FromBase64String(data);
                var tex = new Texture2D(2, 2);
                if (tex.LoadImage(bytes))
                    return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
            catch { }
            return null;
        }
    }
}
