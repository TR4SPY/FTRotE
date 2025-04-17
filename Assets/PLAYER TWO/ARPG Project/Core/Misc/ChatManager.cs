using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using AI_DDA.Assets.Scripts;
using System.Linq;
using Gaia;
using System.Collections.Generic;

namespace PLAYERTWO.ARPGProject
{
    public class ChatManager : MonoBehaviour
    {
        // Input Actions
        private InputActionAsset gameplayActions;
        private InputActionAsset guiActions;
        private InputAction toggleChatAction;

        private InputActionMap gameplayMap;
        private InputActionMap guiMap;

        public string toggleActionName = "Toggle Chat";

        private bool isChatOpen = false;
        private bool wasFocusedLastFrame = false;
        private bool chatWasManuallyOpened = false;

        private static readonly Dictionary<string, List<string>> helpMessages = new()
        {
            { "money", new List<string> { "/money [amount] - Adds money to your inventory." } },
            { "drop", new List<string> { "/drop [group] [id] [level] [attr] [durability] - Drops an item with specified attributes." } },
            { "tp", new List<string> { "/tp x y z - Teleports player to specified coordinates." } },
            { "wp", new List<string> { "/wp [index] - Teleports to waypoint index." } },
            { "listwp", new List<string> { "/listwp - Lists all available waypoints." } },
            { "whereami", new List<string> { "/whereami - Shows your current position." } },
            { "summon", new List<string> { "/summon [enemyId] - Summons enemy by ID." } },
            { "heal", new List<string> { "/heal - Fully restores health and mana." } },
            { "kill", new List<string> { "/kill - Kills your character." } },
            { "addexp", new List<string> { "/addexp [value] - Adds experience points." } },
            { "lvlup", new List<string> { "/lvlup - Increases your character level by one." } },
            { "godmode", new List<string> { "/godmode - Toggles invincibility mode." } },
            { "time", new List<string> { "/time [day|night] - Changes time of day." } },
            { "weather", new List<string> { "/weather [sun|rain|snow] - Changes weather." } },
            { "dda", new List<string>
                {
                    "/dda log - Shows current DDA log.",
                    "/dda reset - Resets AI-DDA system.",
                    "/dda export - Exports player data to CSV.",
                    "/dda force - Forces difficulty recalculation.",
                    "/dda type - Displays current player dynamic type.",
                    "/dda diff [value] - Manually sets difficulty.",
                    "/dda achievements - Lists unlocked achievements.",
                    "/dda quests - Lists completed quests."
                }
            },
            { "clear", new List<string> { "/clear - Clears chat log." } }
        };

        private void Awake()
        {
            gameplayActions = Game.instance.gameplayActions;
            guiActions = Game.instance.guiActions;

            if (gameplayActions == null || guiActions == null)
            {
                Debug.LogError("[ChatManager] Input actions not assigned in Game.cs! Attempting to load manually.");

                gameplayActions = Resources.Load<InputActionAsset>("Entity Controls");
                guiActions = Resources.Load<InputActionAsset>("GUI Actions");

                if (gameplayActions == null || guiActions == null)
                {
                    Debug.LogError("[ChatManager] Critical error: Could not find input actions assets.");
                    return;
                }
            }

            Reinitialize();
        }

        public void Reinitialize()
        {
            gameplayMap = gameplayActions?.FindActionMap("Gameplay");
            guiMap = guiActions?.FindActionMap("GUI");

            if (gameplayMap == null)
                Debug.LogError("[ChatManager] Gameplay input action map not found.");

            if (guiMap == null)
                Debug.LogError("[ChatManager] GUI input action map not found.");

            if (toggleChatAction != null)
                toggleChatAction.performed -= OnToggleChat;

            if (guiMap != null)
            {
                toggleChatAction = guiMap.FindAction(toggleActionName);
                if (toggleChatAction != null)
                {
                    toggleChatAction.performed += OnToggleChat;
                }
                else
                {
                    Debug.LogError("[ChatManager] Toggle Chat action not found in GUI action map.");
                }
            }

            wasFocusedLastFrame = false;
            chatWasManuallyOpened = false;
        }

        private void OnEnable()
        {
            toggleChatAction?.Enable();
        }

        private void OnDisable()
        {
            if (toggleChatAction != null)
                toggleChatAction.performed -= OnToggleChat;
        }

        private void Update()
        {
            var cw = GUIWindowsManager.instance?.chatWindow;
            bool isInputFocused = (cw != null && cw.IsInputFocused);

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

        private void OnToggleChat(InputAction.CallbackContext ctx)
        {
            ToggleChat();
        }

        private void ToggleChat()
        {
            isChatOpen = !isChatOpen;
            var cw = GUIWindowsManager.instance?.chatWindow;

            if (isChatOpen)
            {
                chatWasManuallyOpened = true;
                cw?.Show();
                cw?.FocusInput();
            }
            else
            {
                chatWasManuallyOpened = false;
                cw?.Hide();
                cw?.RemoveFocus();

                gameplayMap?.Enable();
                guiMap?.Enable();
            }
        }

        public void SubmitMessage(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            var cw = GUIWindowsManager.instance?.chatWindow;
            string prefix = StringUtils.StringWithColorAndStyle("You:", GameColors.LightBlue, bold: true);
            cw?.AddMessageToLog($"{prefix} {text}");

            if (text.StartsWith("/"))
                ProcessCommand(text.Substring(1));

            if (cw != null)
            {
                cw.inputField.text = string.Empty;
                cw.FocusInput();
            }
        }

        private void AddSystemMessage(string message)
        {
            var cw = GUIWindowsManager.instance?.chatWindow;
            if (cw == null) return;

            string prefix = StringUtils.StringWithColorAndStyle("[System]", GameColors.Gold, bold: true);
            cw.AddMessageToLog(prefix + " " + message);
        }

        private void AddSystemMessageBatch(IEnumerable<string> messages)
        {
            var cw = GUIWindowsManager.instance?.chatWindow;
            if (cw == null) return;

            string prefix = StringUtils.StringWithColorAndStyle("[System]", GameColors.Gold, bold: true);

            string combined = string.Join("\n", messages);
            cw.AddMessageToLog($"{prefix} {combined}");
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
                {
                    if (parts.Length >= 2)
                    {
                        string command = parts[1].ToLower();

                        if (helpMessages.TryGetValue(command, out var descriptions))
                        {
                            var formattedDescriptions = descriptions.ConvertAll(desc =>
                                StringUtils.StringWithColor(desc, GameColors.Gray)
                            );

                            AddSystemMessageBatch(formattedDescriptions);
                        }
                        else
                        {
                            AddSystemMessage(StringUtils.StringWithColorAndStyle(
                                $"No help available for command: /{command}",
                                GameColors.Crimson,
                                italic: true
                            ));
                        }
                    }
                    else
                    {
                        string message = StringUtils.StringWithColorAndStyle(
                            "Available commands:\n" +
                            "/money, /drop, /tp, /wp, /listwp, /whereami, /summon, /heal, /kill, /addexp, /lvlup, /godmode, /time, /weather, /dda, /clear\n" +
                            "Type /help [command] for more details.",
                            GameColors.Gray,
                            italic: true
                        );

                        AddSystemMessage(message);
                    }
                    break;
                }

                case "money":
                    if (parts.Length >= 2 && int.TryParse(parts[1], out int gold))
                    {
                        inventory.AddGold(gold);
                        AddSystemMessage(StringUtils.StringWithColorAndStyle($"Get rich quick scheme: +{gold} money", GameColors.Gold, bold: true));
                    }
                    break;
                
                case "heal":
                {
                    var entity = character.Entity;

                    if (entity != null && entity.stats != null)
                    {
                        entity.stats.health = entity.stats.maxHealth;
                        entity.stats.mana = entity.stats.maxMana;

                        AddSystemMessage(StringUtils.StringWithColorAndStyle("You feel restored.", GameColors.Cyan));
                    }
                    else
                    {
                        AddSystemMessage(StringUtils.StringWithColor("Failed to heal: character entity not found.", GameColors.Crimson));
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
                                AddSystemMessage(StringUtils.StringWithColorAndStyle($"+{exp} Experience gained", GameColors.Lime));
                            }
                            else
                            {
                                AddSystemMessage(StringUtils.StringWithColor("Entity not found.", GameColors.Crimson));
                            }
                        }
                        else
                        {
                            AddSystemMessage(StringUtils.StringWithColor("Usage: /addexp [amount]", GameColors.Gray));
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
                            AddSystemMessage(StringUtils.StringWithColor("Already at max level!", GameColors.Crimson));
                            break;
                        }

                        int targetLevel = entity.stats.level + amount;
                        if (targetLevel > maxLevel)
                        {
                            int allowedAmount = maxLevel - entity.stats.level;
                            entity.stats.ForceLevelUp(allowedAmount);
                            AddSystemMessage(StringUtils.StringWithColor($"Level capped at {maxLevel}. Gained {allowedAmount} level(s).", GameColors.Orange));
                        }
                        else
                        {
                            entity.stats.ForceLevelUp(amount);
                            AddSystemMessage(StringUtils.StringWithColor($"Leveled up by {amount}. Current level: {entity.stats.level}", GameColors.Orange));
                        }
                    }
                    else
                    {
                        AddSystemMessage(StringUtils.StringWithColor("Entity not found.", GameColors.Crimson));
                    }
                    break;
                }

                case "kill":
                    {
                        var entity = character.Entity;
                        if (entity != null)
                        {
                            entity.stats.health = 0;
                            AddSystemMessage(StringUtils.StringWithColorAndStyle("You feel... dead.", GameColors.Crimson, italic: true));
                        }
                        else
                        {
                            AddSystemMessage(StringUtils.StringWithColor("Entity not found.", GameColors.Crimson));
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
                                AddSystemMessage(StringUtils.StringWithColor("Time set to noon.", GameColors.LightBlue));
                            }
                            else if (time == "night")
                            {
                                tod.m_todHour = 0;
                                tod.m_todMinutes = 0;
                                AddSystemMessage(StringUtils.StringWithColor("Time set to midnight.", GameColors.LightBlue));
                            }
                            Gaia.PWSkyStandalone.Instance.UpdateGaiaTimeOfDay(false);
                        }
                        else
                        {
                            AddSystemMessage(StringUtils.StringWithColor("Gaia system not found.", GameColors.Crimson));
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
                                    AddSystemMessage(StringUtils.StringWithColor("Weather: Sunny", GameColors.Lime));
                                    break;
                                case "rain":
                                    weather.PlayRain();
                                    AddSystemMessage(StringUtils.StringWithColor("Weather: Rainy", GameColors.Cyan));
                                    break;
                                case "snow":
                                    weather.PlaySnow();
                                    AddSystemMessage(StringUtils.StringWithColor("Weather: Snowing", GameColors.Gray));
                                    break;
                                default:
                                    AddSystemMessage(StringUtils.StringWithColor("Unknown weather type. Use sun, rain, or snow.", GameColors.Crimson));
                                    break;
                            }
                        }
                        else
                        {
                            AddSystemMessage(StringUtils.StringWithColor("Weather system not found.", GameColors.Crimson));
                        }
                    }
                    else
                    {
                        AddSystemMessage(StringUtils.StringWithColor("Usage: /weather [sun|rain|snow]", GameColors.Gray));
                    }
                #else
                    AddSystemMessage(StringUtils.StringWithColor("Weather system requires GAIA_2023_PRO define.", GameColors.Crimson));
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
                        AddSystemMessage(StringUtils.StringWithColor($"Godmode: {state}", GameColors.Gold));
                    }
                    else
                    {
                        AddSystemMessage(StringUtils.StringWithColor("Godmode failed: stats not found.", GameColors.Crimson));
                    }
                    break;
                }
            
                case "clear":
                {
                    var cw = GUIWindowsManager.instance?.chatWindow;
                    cw?.ClearLog();
                    //AddSystemMessage(StringUtils.StringWithColor("Chat log cleared.", GameColors.Gray));
                    break;
                }

                case "drop":
                {
                    int level = 0;
                    int attributes = 0;
                    int durabilityPercent = 100;
                    int stackAmount = 1;

                    if (parts.Length >= 3 &&
                        int.TryParse(parts[1], out int group) &&
                        int.TryParse(parts[2], out int idInGroup))
                    {
                        if (parts.Length >= 4) int.TryParse(parts[3], out level);
                        if (parts.Length >= 5) int.TryParse(parts[4], out attributes);
                        if (parts.Length >= 6) int.TryParse(parts[5], out durabilityPercent);
                        if (parts.Length >= 7) int.TryParse(parts[6], out stackAmount);

                        stackAmount = Mathf.Max(1, stackAmount);

                        int combinedID = int.Parse($"{group}{idInGroup}");

                        var item = GameDatabase.instance.items
                            .FirstOrDefault(i => i.id == combinedID);

                        if (item == null)
                        {
                            AddSystemMessage(StringUtils.StringWithColor($"Item not found (Group {group}, ID {idInGroup})", GameColors.Crimson));
                            break;
                        }

                        bool isStackable = item.canStack;

                        for (int i = 0; i < (isStackable ? 1 : stackAmount); i++)
                        {
                            int actualStack = isStackable ? stackAmount : 1;

                            var instance = new ItemInstance(item, false);
                            instance.ForceStack(actualStack);

                            level = Mathf.Clamp(level, 0, 25);
                            for (int j = 0; j < level; j++)
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
                        }

                        AddSystemMessage(StringUtils.StringWithColorAndStyle(
                            $"Dropped {item.GetName()} (+{level}, {attributes} attr, {durabilityPercent}% durability, stack {stackAmount})",
                            GameColors.Orange));
                    }
                    else
                    {
                        AddSystemMessage(StringUtils.StringWithColor(
                            "Usage: /drop [group] [idInGroup] [level] [attributes] [durability] [stack]",
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
                        AddSystemMessage(StringUtils.StringWithColorAndStyle($"Teleported to {x}, {y}, {z}", GameColors.LightBlue, italic: true));
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
                            AddSystemMessage(StringUtils.StringWithColorAndStyle($"Warped to {waypoint.title}", GameColors.LightBlue));
                        }
                        else
                        {
                            AddSystemMessage(StringUtils.StringWithColor("Invalid waypoint index.", GameColors.Crimson));
                        }
                    }
                    else
                    {
                        AddSystemMessage(StringUtils.StringWithColor("/wp [index]", GameColors.Gray));
                    }
                    break;


                    case "listwp":
                        var waypoints = LevelWaypoints.instance.waypoints;
                        var lines = new List<string>();

                        for (int i = 0; i < waypoints.Count; i++)
                        {
                            var wp = waypoints[i];
                            lines.Add(StringUtils.StringWithColor($"[{i}] - {wp.title}", GameColors.Gray));
                        }

                        AddSystemMessageBatch(lines);
                        break;

                    case "whereami":
                        var pos = Level.instance.player.transform.position;
                        AddSystemMessage($"You are at: X={pos.x:F2} Y={pos.y:F2} Z={pos.z:F2}");
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
                            AddSystemMessage(StringUtils.StringWithColorAndStyle($"Summoned enemy {enemyId}", GameColors.Orange));
                        }
                        else
                        {
                            AddSystemMessage(StringUtils.StringWithColor($"Enemy ID {enemyId} is out of range.", GameColors.Crimson));
                        }
                    }
                    else
                    {
                        AddSystemMessage(StringUtils.StringWithColor("/summon [enemyId]", GameColors.Gray));
                    }
                    break;

                    case "dda":
                        if (parts.Length == 1)
                        {
                            AddSystemMessage(StringUtils.StringWithColor("/dda [log/reset/export/force/type/difficulty]", GameColors.Gray));
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
                                    AddSystemMessage(StringUtils.StringWithColor(msg, GameColors.LightBlue));
                                }
                                else
                                {
                                    AddSystemMessage(StringUtils.StringWithColor("Logger not initialized.", GameColors.Crimson));
                                }
                                break;

                            case "reset":
                                if (logger != null)
                                {
                                    logger.ResetData();
                                    AddSystemMessage(StringUtils.StringWithColor("Player behavior stats reset.", GameColors.Orange));
                                }
                                break;

                            case "export":
                                if (logger != null)
                                {
                                    logger.ExportPlayerData();
                                    AddSystemMessage(StringUtils.StringWithColor("Exported player data to CSV.", GameColors.Lime));
                                }
                                break;

                            case "force":
                                {
                                    var rl = RLModel.Instance;
                                    if (rl == null)
                                    {
                                        AddSystemMessage(StringUtils.StringWithColor("RLModel not initialized.", GameColors.Crimson));
                                        break;
                                    }

                                    float current = rl.GetCurrentDifficulty();
                                    rl.AdjustDifficulty(current);

                                    AddSystemMessage(StringUtils.StringWithColorAndStyle($"[AI-DDA] Requested difficulty recalculation (base {current})", GameColors.LightBlue));
                                    break;
                                }

                            case "type":
                                if (logger != null)
                                {
                                    AddSystemMessage(StringUtils.StringWithColor($"Dynamic player Bartle's type: {logger.currentDynamicPlayerType}", GameColors.Cyan));
                                }
                                break;

                            case "diff":
                                if (parts.Length >= 3 && float.TryParse(parts[2], out float val))
                                {
                                    if (rlModel != null)
                                    {
                                        rlModel.SetCurrentDifficulty(val);
                                        AddSystemMessage(StringUtils.StringWithColor($"Difficulty manually set to {val}", GameColors.Orange));
                                    }
                                }
                                else
                                {
                                    AddSystemMessage(StringUtils.StringWithColor("Usage: /dda diff [value]", GameColors.Gray));
                                }
                                break;

                            case "achievements":
                                if (logger != null)
                                {
                                    if (logger.unlockedAchievements.Count > 0)
                                    {
                                        AddSystemMessage(StringUtils.StringWithColor($"Unlocked Achievements ({logger.unlockedAchievements.Count}):", GameColors.Lime));
                                        foreach (var a in logger.unlockedAchievements)
                                            AddSystemMessage(StringUtils.StringWithColor($"• {a}", GameColors.Gold));
                                    }
                                    else
                                    {
                                        AddSystemMessage(StringUtils.StringWithColor("No achievements unlocked yet.", GameColors.Gray));
                                    }
                                }
                                else
                                {
                                    AddSystemMessage(StringUtils.StringWithColor("Logger not initialized.", GameColors.Crimson));
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
                                        AddSystemMessage("No quests completed yet.");
                                    }
                                    else
                                    {
                                        AddSystemMessage(StringUtils.StringWithColor("Completed Quests:", GameColors.Cyan));
                                        foreach (var quest in completed)
                                        {
                                            AddSystemMessage($"✔ {quest.data.title}");
                                        }
                                    }
                                }
                                else
                                {
                                    AddSystemMessage(StringUtils.StringWithColor("Quest system not initialized.", GameColors.Crimson));
                                }
                                break;

                            default:
                                AddSystemMessage(StringUtils.StringWithColor($"Unknown /dda command: {sub}", GameColors.Crimson));
                                break;
                        }
                        break;

                default:
                    AddSystemMessage(StringUtils.StringWithColor($"Unknown command: /{cmd}", GameColors.Crimson));
                    break;
            }
        }
    }
}
