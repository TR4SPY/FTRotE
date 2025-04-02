using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.EventSystems;
using Gaia;
using AI_DDA.Assets.Scripts;

namespace PLAYERTWO.ARPGProject
{
    public class ChatManager : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject chatPanel;
        public TMP_InputField inputField;
        public Transform logContent;
        public GameObject messagePrefab;

        [Header("Options")]
        public int maxMessages = 50;
        public InputActionAsset inputActions;
        public InputActionAsset gameplayActions;
        public InputActionAsset guiActions;
        public string toggleActionName = "Toggle Chat";

        private bool isChatOpen = false;
        private bool wasFocusedLastFrame = false;
        private bool chatWasManuallyOpened = false;

        private Queue<GameObject> messageQueue = new();
        private InputAction toggleChatAction;
        private InputActionMap gameplayMap;
        private InputActionMap guiMap;

        private void Awake()
        {
            gameplayMap = gameplayActions.FindActionMap("Gameplay");
            guiMap = guiActions.FindActionMap("GUI");

            toggleChatAction = guiMap.FindAction(toggleActionName);
            toggleChatAction.performed += ctx => ToggleChat();

            inputField.onSubmit.AddListener(OnSubmitMessage);
        }

        private void OnEnable() => toggleChatAction.Enable();
        private void OnDisable() => toggleChatAction.Disable();

        private void Update()
        {
            bool isInputFocused = inputField != null && inputField.isFocused;

            if (isInputFocused && !wasFocusedLastFrame)
            {
                gameplayMap?.Disable();
                guiMap?.Disable();
            }
            else if (!isInputFocused && wasFocusedLastFrame)
            {
                if (chatWasManuallyOpened)
                {
                    guiMap?.Enable();
                }
                gameplayMap?.Enable();
            }

            wasFocusedLastFrame = isInputFocused;
        }

        private void OnSubmitMessage(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            SubmitMessage(text.Trim());
            inputField.text = "";

            inputField.ActivateInputField();
            inputField.Select();
            EventSystem.current.SetSelectedGameObject(inputField.gameObject);
        }

        private void ToggleChat()
        {
            isChatOpen = !isChatOpen;

            if (isChatOpen)
            {
                chatWasManuallyOpened = true;

                GUIWindowsManager.instance.GetChatWindow()?.Show();

                inputField.text = "";
                inputField.ActivateInputField();
                inputField.Select();
                EventSystem.current.SetSelectedGameObject(inputField.gameObject);
            }
            else
            {
                chatWasManuallyOpened = false;

                GUIWindowsManager.instance.GetChatWindow()?.Hide();
                inputField.DeactivateInputField();
                EventSystem.current.SetSelectedGameObject(null);

                gameplayMap?.Enable();
                guiMap?.Enable();
            }
        }

        private void SubmitMessage(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            string prefix = StringUtils.StringWithColorAndStyle("You:", GameColors.LightBlue, bold: true);
            AddMessageToLog($"{prefix} {text}");

            if (text.StartsWith("/"))
                ProcessCommand(text.Substring(1));
        }

        private void AddMessageToLog(string formattedMessage)
        {
            if (messagePrefab == null || logContent == null)
            {
                Debug.LogWarning("[ChatManager] Missing messagePrefab or logContent!");
                return;
            }

            var obj = Instantiate(messagePrefab, logContent);

            var tmpLabel = obj.GetComponentInChildren<TMPro.TMP_Text>();
            if (tmpLabel != null)
            {
                tmpLabel.text = formattedMessage;
            }
            else
            {
                var uiLabel = obj.GetComponentInChildren<UnityEngine.UI.Text>();
                if (uiLabel != null)
                {
                    uiLabel.text = formattedMessage;
                }
                else
                {
                    Debug.LogWarning("[ChatManager] No text component found on messagePrefab!");
                }
            }

            messageQueue.Enqueue(obj);

            if (messageQueue.Count > maxMessages)
            {
                var old = messageQueue.Dequeue();
                Destroy(old);
            }
        }

        private void ProcessCommand(string commandLine)
        {
            string[] parts = commandLine.Split(' ');
            string cmd = parts[0].ToLower();

            var character = Game.instance.currentCharacter;
            var inventory = character.inventory;

            switch (cmd)
            {
                case "help":
                    AddMessageToLog(StringUtils.StringWithColorAndStyle(
                        "/money [amount], /drop [group] [id] [level] [attr] [durability], /tp x y z, /wp [index], /listwp, /whereami, /summon [enemyId]",
                        GameColors.Gray, italic: true));

                    AddMessageToLog(StringUtils.StringWithColorAndStyle(
                        "/heal, /kill, /addexp [value], /lvlup, /godmode, /time [day|night], /weather [sun|rain|snow]",
                        GameColors.Gray, italic: true));

                    AddMessageToLog(StringUtils.StringWithColorAndStyle(
                        "/dda [log|reset|export|force|type|diff value|achievements|quests], /clear",
                        GameColors.Gray, italic: true));
                    break;

                case "money":
                    if (parts.Length >= 2 && int.TryParse(parts[1], out int gold))
                    {
                        inventory.AddGold(gold);
                        AddMessageToLog(StringUtils.StringWithColorAndStyle($"Get rich quick scheme: +{gold} money", GameColors.Gold, bold: true));
                    }
                    break;
                
                case "heal":
                {
                    var entity = character.Entity;

                    if (entity != null && entity.stats != null)
                    {
                        entity.stats.health = entity.stats.maxHealth;
                        entity.stats.mana = entity.stats.maxMana;

                        AddMessageToLog(StringUtils.StringWithColorAndStyle("You feel restored.", GameColors.Cyan));
                    }
                    else
                    {
                        AddMessageToLog(StringUtils.StringWithColor("Failed to heal: character entity not found.", GameColors.Crimson));
                    }

                    break;
                }

                case "addexp":
                    {
                        if (parts.Length >= 2 && int.TryParse(parts[1], out int exp))
                        {
                            var entity = character.Entity;
                            if (entity != null)
                            {
                                entity.stats.AddExperience(exp);
                                AddMessageToLog(StringUtils.StringWithColorAndStyle($"+{exp} Experience gained", GameColors.Lime));
                            }
                            else
                            {
                                AddMessageToLog(StringUtils.StringWithColor("Entity not found.", GameColors.Crimson));
                            }
                        }
                        else
                        {
                            AddMessageToLog(StringUtils.StringWithColor("Usage: /addexp [amount]", GameColors.Gray));
                        }
                        break;
                    }

                case "lvlup":
                {
                    var entity = character.Entity;
                    if (entity != null)
                    {
                        int amount = 1;
                        if (parts.Length >= 2)
                            int.TryParse(parts[1], out amount);

                        int maxLevel = Game.instance.maxLevel;

                        if (entity.stats.level >= maxLevel)
                        {
                            AddMessageToLog(StringUtils.StringWithColor("Already at max level!", GameColors.Crimson));
                            break;
                        }

                        int targetLevel = entity.stats.level + amount;
                        if (targetLevel > maxLevel)
                        {
                            int allowedAmount = maxLevel - entity.stats.level;
                            entity.stats.ForceLevelUp(allowedAmount);
                            AddMessageToLog(StringUtils.StringWithColor($"Level capped at {maxLevel}. Gained {allowedAmount} level(s).", GameColors.Orange));
                        }
                        else
                        {
                            entity.stats.ForceLevelUp(amount);
                            AddMessageToLog(StringUtils.StringWithColor($"Leveled up by {amount}. Current level: {entity.stats.level}", GameColors.Orange));
                        }
                    }
                    else
                    {
                        AddMessageToLog(StringUtils.StringWithColor("Entity not found.", GameColors.Crimson));
                    }
                    break;
                }

                case "kill":
                    {
                        var entity = character.Entity;
                        if (entity != null)
                        {
                            entity.stats.health = 0;
                            AddMessageToLog(StringUtils.StringWithColorAndStyle("You feel... dead.", GameColors.Crimson, italic: true));
                        }
                        else
                        {
                            AddMessageToLog(StringUtils.StringWithColor("Entity not found.", GameColors.Crimson));
                        }
                        break;
                    }

                case "time":
                    if (parts.Length >= 2)
                    {
                        string time = parts[1].ToLower();
                        if (Gaia.PWSkyStandalone.Instance != null)
                        {
                            var tod = Gaia.PWSkyStandalone.Instance.m_gaiaTimeOfDay;
                            if (time == "day")
                            {
                                tod.m_todHour = 12;
                                tod.m_todMinutes = 0;
                                AddMessageToLog(StringUtils.StringWithColor("Time set to noon.", GameColors.LightBlue));
                            }
                            else if (time == "night")
                            {
                                tod.m_todHour = 0;
                                tod.m_todMinutes = 0;
                                AddMessageToLog(StringUtils.StringWithColor("Time set to midnight.", GameColors.LightBlue));
                            }
                            Gaia.PWSkyStandalone.Instance.UpdateGaiaTimeOfDay(false);
                        }
                        else
                        {
                            AddMessageToLog(StringUtils.StringWithColor("Gaia system not found.", GameColors.Crimson));
                        }
                    }
                    break;

                case "weather":
                {
                #if GAIA_2023_PRO
                    if (parts.Length >= 2)
                    {
                        var weather = ProceduralWorldsGlobalWeather.Instance;
                        if (weather != null)
                        {
                            string weatherType = parts[1].ToLower();
                            switch (weatherType)
                            {
                                case "sun":
                                    weather.StopRain();
                                    weather.StopSnow();
                                    AddMessageToLog(StringUtils.StringWithColor("Weather: Sunny", GameColors.Lime));
                                    break;
                                case "rain":
                                    weather.PlayRain();
                                    AddMessageToLog(StringUtils.StringWithColor("Weather: Rainy", GameColors.Cyan));
                                    break;
                                case "snow":
                                    weather.PlaySnow();
                                    AddMessageToLog(StringUtils.StringWithColor("Weather: Snowing", GameColors.Gray));
                                    break;
                                default:
                                    AddMessageToLog(StringUtils.StringWithColor("Unknown weather type. Use sun, rain, or snow.", GameColors.Crimson));
                                    break;
                            }
                        }
                        else
                        {
                            AddMessageToLog(StringUtils.StringWithColor("Weather system not found.", GameColors.Crimson));
                        }
                    }
                    else
                    {
                        AddMessageToLog(StringUtils.StringWithColor("Usage: /weather [sun|rain|snow]", GameColors.Gray));
                    }
                #else
                    AddMessageToLog(StringUtils.StringWithColor("Weather system requires GAIA_2023_PRO define.", GameColors.Crimson));
                #endif
                    break;
                }
                
                case "godmode":
                {
                    var entity = character.Entity;

                    if (entity != null && entity.stats != null)
                    {
                        entity.stats.infiniteHealth = !entity.stats.infiniteHealth;
                        entity.stats.infiniteMana = entity.stats.infiniteHealth;

                        string state = entity.stats.infiniteHealth ? "ON" : "OFF";
                        AddMessageToLog(StringUtils.StringWithColor($"Godmode: {state}", GameColors.Gold));
                    }
                    else
                    {
                        AddMessageToLog(StringUtils.StringWithColor("Godmode failed: stats not found.", GameColors.Crimson));
                    }
                    break;
                }
            
                case "clear":
                    foreach (Transform child in logContent)
                        Destroy(child.gameObject);
                    messageQueue.Clear();
                    AddMessageToLog(StringUtils.StringWithColor("Chat log cleared.", GameColors.Gray));
                    break;

                case "drop":
                {
                    int level = 0;
                    int attributes = 0;
                    int durabilityPercent = 100;

                    if (parts.Length >= 3 &&
                        int.TryParse(parts[1], out int group) &&
                        int.TryParse(parts[2], out int idInGroup))
                    {
                        if (parts.Length >= 4) int.TryParse(parts[3], out level);
                        if (parts.Length >= 5) int.TryParse(parts[4], out attributes);
                        if (parts.Length >= 6) int.TryParse(parts[5], out durabilityPercent);

                        int combinedID = int.Parse($"{group}{idInGroup}");

                        var item = GameDatabase.instance.items
                            .FirstOrDefault(i => i.id == combinedID);

                        if (item == null)
                        {
                            AddMessageToLog(StringUtils.StringWithColor($"Item not found (Group {group}, ID {idInGroup})", GameColors.Crimson));
                            break;
                        }

                        var instance = new ItemInstance(item, false);

                        level = Mathf.Clamp(level, 0, 25);
                        for (int i = 0; i < level; i++)
                            instance.UpgradeLevel();

                        if (attributes > 0)
                        {
                            instance.GenerateAttributes();

                            var method = typeof(ItemInstance).GetMethod(
                                "GenerateAdditionalAttributes",
                                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
                            );

                            method?.Invoke(instance, new object[] { attributes, attributes });
                        }

                        float percent = Mathf.Clamp01((float)durabilityPercent / 100f);
                        var durabilityField = typeof(ItemInstance).GetField(
                            "m_durability",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
                        );

                        if (durabilityField != null && item is ItemEquippable eq)
                        {
                            int durabilityValue = Mathf.RoundToInt(eq.maxDurability * percent);
                            durabilityField.SetValue(instance, durabilityValue);
                        }

                        GUI.instance.DropItem(instance);
                        AddMessageToLog(StringUtils.StringWithColorAndStyle(
                            $"Dropped {item.GetName()} (+{level}, {attributes} attr, {durabilityPercent}% durability)",
                            GameColors.Orange));
                    }
                    else
                    {
                        AddMessageToLog(StringUtils.StringWithColor(
                            "Usage: /drop [group] [idInGroup] [level] [attributes] [durability]",
                            GameColors.Gray));
                    }
                    break;
                }

                case "tp":
                    if (parts.Length == 4 &&
                        float.TryParse(parts[1], out float x) &&
                        float.TryParse(parts[2], out float y) &&
                        float.TryParse(parts[3], out float z))
                    {
                        Level.instance.player.transform.position = new Vector3(x, y, z);
                        AddMessageToLog(StringUtils.StringWithColorAndStyle($"Teleported to {x}, {y}, {z}", GameColors.LightBlue, italic: true));
                    }
                    break;

                case "wp":
                    if (parts.Length == 2 && int.TryParse(parts[1], out int wpIndex))
                    {
                        var wpList = LevelWaypoints.instance.waypoints;
                        if (wpIndex >= 0 && wpIndex < wpList.Count)
                        {
                            var waypoint = wpList[wpIndex];
                            LevelWaypoints.instance.TravelTo(waypoint);
                            AddMessageToLog(StringUtils.StringWithColorAndStyle($"Warped to {waypoint.title}", GameColors.LightBlue));
                        }
                        else
                        {
                            AddMessageToLog(StringUtils.StringWithColor("Invalid waypoint index.", GameColors.Crimson));
                        }
                    }
                    else
                    {
                        AddMessageToLog(StringUtils.StringWithColor("/wp [index]", GameColors.Gray));
                    }
                    break;


                    case "listwp":
                        var waypoints = LevelWaypoints.instance.waypoints;
                        for (int i = 0; i < waypoints.Count; i++)
                        {
                            var wp = waypoints[i];
                            AddMessageToLog(StringUtils.StringWithColor($"[{i}] - {wp.title}", GameColors.Gray));
                        }
                        break;

                    case "whereami":
                        var pos = Level.instance.player.transform.position;
                        AddMessageToLog($"You are at: X={pos.x:F2} Y={pos.y:F2} Z={pos.z:F2}");
                        break;

                case "summon":
                    if (parts.Length >= 2 && int.TryParse(parts[1], out int enemyId))
                    {
                        var enemies = GameDatabase.instance.enemies;
                        if (enemyId >= 0 && enemyId < enemies.Count)
                        {
                            var prefab = enemies[enemyId];
                            var position = Level.instance.player.transform.position + Vector3.forward * 2f;
                            GameObject.Instantiate(prefab, position, Quaternion.identity);
                            AddMessageToLog(StringUtils.StringWithColorAndStyle($"Summoned enemy {enemyId}", GameColors.Orange));
                        }
                        else
                        {
                            AddMessageToLog(StringUtils.StringWithColor($"Enemy ID {enemyId} is out of range.", GameColors.Crimson));
                        }
                    }
                    else
                    {
                        AddMessageToLog(StringUtils.StringWithColor("/summon [enemyId]", GameColors.Gray));
                    }
                    break;

                    case "dda":
                        if (parts.Length == 1)
                        {
                            AddMessageToLog(StringUtils.StringWithColor("/dda [log/reset/export/force/type/difficulty]", GameColors.Gray));
                            break;
                        }

                        string sub = parts[1].ToLower();
                        var logger = PlayerBehaviorLogger.Instance;
                        var rlModel = RLModel.Instance;
                        var mlModel = AIModel.Instance;

                        switch (sub)
                        {
                            case "log":
                                if (logger != null)
                                {
                                    string playtimeFormatted = PlayerBehaviorLogger.FormatPlayTime(character.totalPlayTime);

                                    string msg = $"[AI-DDA] Player Deaths: {logger.playerDeaths}, Enemies Defeated: {logger.enemiesDefeated}, Potions Used: {logger.potionsUsed}, Zones Discovered: {logger.zonesDiscovered}, " +
                                                $"Play Time: {playtimeFormatted}, Combat Time: {logger.totalCombatTime:F1}s, Quests Completed: {logger.questsCompleted}, " +
                                                $"Achievements: {logger.unlockedAchievements.Count}, Current Bartle's Type: {logger.currentDynamicPlayerType}";
                                    AddMessageToLog(StringUtils.StringWithColor(msg, GameColors.LightBlue));
                                }
                                else
                                {
                                    AddMessageToLog(StringUtils.StringWithColor("Logger not initialized.", GameColors.Crimson));
                                }
                                break;

                            case "reset":
                                if (logger != null)
                                {
                                    logger.ResetData();
                                    AddMessageToLog(StringUtils.StringWithColor("Player behavior stats reset.", GameColors.Orange));
                                }
                                break;

                            case "export":
                                if (logger != null)
                                {
                                    logger.ExportPlayerData();
                                    AddMessageToLog(StringUtils.StringWithColor("Exported player data to CSV.", GameColors.Lime));
                                }
                                break;

                            case "force":
                                {
                                    var rl = RLModel.Instance;
                                    if (rl == null)
                                    {
                                        AddMessageToLog(StringUtils.StringWithColor("RLModel not initialized.", GameColors.Crimson));
                                        break;
                                    }

                                    float current = rl.GetCurrentDifficulty();
                                    rl.AdjustDifficulty(current);

                                    AddMessageToLog(StringUtils.StringWithColorAndStyle($"[AI-DDA] Requested difficulty recalculation (base {current})", GameColors.LightBlue));
                                    break;
                                }

                            case "type":
                                if (logger != null)
                                {
                                    AddMessageToLog(StringUtils.StringWithColor($"Dynamic player Bartle's type: {logger.currentDynamicPlayerType}", GameColors.Cyan));
                                }
                                break;

                            case "diff":
                                if (parts.Length >= 3 && float.TryParse(parts[2], out float val))
                                {
                                    if (rlModel != null)
                                    {
                                        rlModel.SetCurrentDifficulty(val);
                                        AddMessageToLog(StringUtils.StringWithColor($"Difficulty manually set to {val}", GameColors.Orange));
                                    }
                                }
                                else
                                {
                                    AddMessageToLog(StringUtils.StringWithColor("Usage: /dda diff [value]", GameColors.Gray));
                                }
                                break;

                            case "achievements":
                                if (logger != null)
                                {
                                    if (logger.unlockedAchievements.Count > 0)
                                    {
                                        AddMessageToLog(StringUtils.StringWithColor($"Unlocked Achievements ({logger.unlockedAchievements.Count}):", GameColors.Lime));
                                        foreach (var a in logger.unlockedAchievements)
                                            AddMessageToLog(StringUtils.StringWithColor($"• {a}", GameColors.Gold));
                                    }
                                    else
                                    {
                                        AddMessageToLog(StringUtils.StringWithColor("No achievements unlocked yet.", GameColors.Gray));
                                    }
                                }
                                else
                                {
                                    AddMessageToLog(StringUtils.StringWithColor("Logger not initialized.", GameColors.Crimson));
                                }
                                break;


                            case "quests":
                                var questData = Game.instance.currentCharacter.quests;

                                if (questData != null && questData.currentQuests != null)
                                {
                                    var completed = questData.currentQuests
                                        .Where(q => q != null && q.completed)
                                        .ToList();

                                    if (completed.Count == 0)
                                    {
                                        AddMessageToLog("No quests completed yet.");
                                    }
                                    else
                                    {
                                        AddMessageToLog(StringUtils.StringWithColor("Completed Quests:", GameColors.Cyan));
                                        foreach (var quest in completed)
                                        {
                                            AddMessageToLog($"✔ {quest.data.title}");
                                        }
                                    }
                                }
                                else
                                {
                                    AddMessageToLog(StringUtils.StringWithColor("Quest system not initialized.", GameColors.Crimson));
                                }
                                break;

                            default:
                                AddMessageToLog(StringUtils.StringWithColor($"Unknown /dda command: {sub}", GameColors.Crimson));
                                break;
                        }
                        break;

                default:
                    AddMessageToLog(StringUtils.StringWithColor($"Unknown command: /{cmd}", GameColors.Crimson));
                    break;
            }
        }
    }
}
