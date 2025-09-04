using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AI_DDA.Assets.Scripts;
using System.Collections;
using System.Linq;
using System;
using Gaia;

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

        private List<string> messageHistory = new();

        public static ChatManager Instance { get; private set; }

        private static readonly Dictionary<string, List<string>> helpMessages = new()
        {
            { "money", new List<string> { "/money [amount] - Adds money to your inventory." } },
            { "drop", new List<string> { "/drop [group] [id] [level] [attr] [elements] [durability] [hasSkill] [stack] - Drops an item." } },
            { "tp", new List<string> { "/tp [Z] [X] [Y] - Teleports player to specified coordinates (North/South, East/West, Height)." } },
            { "wp", new List<string> { "/wp [index] - Teleports to waypoint index." } },
            { "listwp", new List<string> { "/listwp - Lists all available waypoints." } },
            { "whereami", new List<string> { "/whereami - Shows your current position and region." } },
            { "summon", new List<string> { "/summon [enemyId] - Summons enemy by ID." } },
            { "heal", new List<string> { "/heal - Fully restores health and mana." } },
            { "kill", new List<string> { "/kill - Kills your character." } },
            { "buff", new List<string> { "/buff [id] - Applies a buff to your character by ID." } },
            { "addexp", new List<string> { "/addexp [value] - Adds experience points." } },
            { "lvlup", new List<string> { "/lvlup - Increases your character level by one." } },
            { "repairall", new List<string> { "/repairall - Repairs all items in your inventory and equipment." } },
            { "stats", new List<string> { "/stats - Displays current player statistics." } },
            { "godmode", new List<string> { "/godmode - Toggles invincibility mode." } },
            { "gremove", new List<string> { "/gremove - Removes current guild." } },
            { "pause", new List<string> { "/pause - Pause the game." } },
            { "resume", new List<string> { "/resume - Resume the game." } },
            { "time", new List<string> { "/time [day|night] - Changes time of day." } },
            { "weather", new List<string> { "/weather [sun|rain|snow] - Changes weather." } },
            { "dda", new List<string>
                {
                    "Available commands for Dynamic Difficulty Adjustment",
                    "/dda log - Shows current DDA log.",
                    "/dda reset - Resets AI-DDA system.",
                    "/dda toggle - Toggle AI-DDA system.",
                    "/dda export - Exports player data to CSV.",
                    "/dda force - Forces difficulty recalculation.",
                    "/dda type - Displays current player dynamic type.",
                    "/dda diff [value] - Manually sets difficulty.",
                    "/dda free - Give back control to the AI models."
                }
            },
            { "achievements", new List<string> { "/achievements - Lists unlocked achievements." } },
            { "zones", new List<string> { "/zones - Lists discovered zones." } },
            { "quests", new List<string> { "/quests - Lists completed quests." } },
            { "info", new List<string> { "/info - Shows game version and build date." } },
            { "clear", new List<string> { "/clear - Clears chat log." } }
        };

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

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
	   
	    toggleChatAction?.Enable();
	    guiMap?.Enable();
        }

        private void OnEnable()
        {
	    guiMap?.Enable();
	    gameplayMap?.Enable();
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

            // Debug.Log($"[ToggleChat] isChatOpen = {isChatOpen}");
            // Debug.Log($"[ToggleChat] chatWindow = {cw}");

            if (isChatOpen)
            {
                chatWasManuallyOpened = true;

                if (cw == null)
                {
                    Debug.LogError("[ToggleChat] ChatWindow is null!");
                }
                else
                {
                    cw?.Show();
                    cw?.FocusInput();
                    
                    var recentMessages = GetLog();
                    cw?.RepopulateOverlayFromHistory(recentMessages, maxToShow: cw.overlayMaxMessages);
                    cw?.ScrollOverlayToBottom();

                    //cw?.SetOverlayVisible(true);
                    //cw?.StopOverlayFadeOut();
                }
            }
            else
            {
                chatWasManuallyOpened = false;

                // Debug.Log($"[ChatManager] Closing chat. cw = {cw}, overlayController = {cw?.overlayController}");

                cw?.Hide();
                cw?.RemoveFocus();
                cw?.overlayController?.ResumeFadeForAllVisible(); // <=== TO DODAJ

                // Debug.Log("[ToggleChat] cw is " + (cw == null ? "NULL" : "NOT NULL"));

                gameplayMap?.Enable();
                guiMap?.Enable();
            }
        }

        public bool IsChatOpen => isChatOpen;

        public void SubmitMessage(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            var cw = GUIWindowsManager.instance?.chatWindow;
            string time = StringUtils.StringWithColor($"[{DateTime.Now:HH:mm}]", GameColors.SlateGray);
            string prefix = StringUtils.StringWithColorAndStyle("You:", GameColors.LightBlue, bold: true);
            string displayText = text;
            if (GameSettings.instance != null && GameSettings.instance.GetBlockProfanity())
            {
                displayText = FilterProfanity(displayText);
            }

            string formatted = $"{time} {prefix} {displayText}";

            // cw?.AddMessageToLog(formatted);
            cw?.AddOverlayMessage(formatted);

            messageHistory.Add(formatted);
            if (cw != null && messageHistory.Count > cw.overlayMaxMessages)
                messageHistory.RemoveAt(0);

            if (text.StartsWith("/"))
                ProcessCommand(text.Substring(1));

            if (cw != null)
            {
                cw.inputField.text = string.Empty;
                cw.FocusInput();
            }
        }

        public List<string> GetLog()
        {
            return messageHistory.ToList();
        }

        private void AddSystemMessage(string message)
        {
            var cw = GUIWindowsManager.instance?.chatWindow;
            if (cw == null) return;

            string time = StringUtils.StringWithColor($"[{DateTime.Now:HH:mm}]", GameColors.White);
            string prefix = StringUtils.StringWithColorAndStyle("[System]", GameColors.Gold, bold: true);
            string formatted = $"{time} {prefix} {message}";

            // cw.AddMessageToLog(formatted);
            cw.AddOverlayMessage(formatted);

            messageHistory.Add(formatted);
            if (cw != null && messageHistory.Count > cw.overlayMaxMessages)
                messageHistory.RemoveAt(0);
        }

        private void AddSystemMessageBatch(List<string> messages)
        {
            var cw = GUIWindowsManager.instance?.chatWindow;
            if (cw == null || messages == null || messages.Count == 0) return;

            string time = StringUtils.StringWithColor($"[{DateTime.Now:HH:mm}]", GameColors.White);
            string prefix = StringUtils.StringWithColorAndStyle("[System]", GameColors.Gold, bold: true);
            string combined = string.Join("\n", messages);
            string formatted = $"{time} {prefix} {combined}";

            // cw.AddMessageToLog(formatted);
            cw.AddOverlayMessage(formatted);

            messageHistory.Add(formatted);
            if (cw != null && messageHistory.Count > cw.overlayMaxMessages)
                messageHistory.RemoveAt(0);
        }

        private string FilterProfanity(string input)
        {
            var banned = new[] { "badword" };
            string result = input;
            foreach (var word in banned)
            {
                var pattern = Regex.Escape(word);
                result = Regex.Replace(result, pattern, new string('*', word.Length), RegexOptions.IgnoreCase);
            }
            return result;
        }

        public static List<string> BuildStatsLines(EntityStatsManager stats)
        {
            var lines = new List<string>();
            if (stats == null)
            {
                lines.Add(StringUtils.StringWithColor("Stats unavailable.", GameColors.Crimson));
                return lines;
            }

            string Label(string label) => StringUtils.StringWithColor($"  {label}: ", GameColors.Gold);
            string Val(string value) => StringUtils.StringWithColor(value, GameColors.White);

            lines.Add($"{Label("Level")}{Val(stats.level.ToString())}");
            lines.Add($"{Label("Exp")}{Val($"{stats.experience}/{stats.nextLevelExp}")}");
            lines.Add($"{Label("Health")}{Val($"{stats.health}/{stats.maxHealth}")}");
            lines.Add($"{Label("Mana")}{Val($"{stats.mana}/{stats.maxMana}")}");
            lines.Add($"{Label("Strength")}{Val(stats.strength.ToString())}");
            lines.Add($"{Label("Dexterity")}{Val(stats.dexterity.ToString())}");
            lines.Add($"{Label("Vitality")}{Val(stats.vitality.ToString())}");
            lines.Add($"{Label("Energy")}{Val(stats.energy.ToString())}");
            lines.Add($"{Label("Damage")}{Val($"{stats.minDamage}-{stats.maxDamage}")}");
            lines.Add($"{Label("Defense")}{Val(stats.defense.ToString())}");
            lines.Add($"{Label("Attack Speed")}{Val(stats.attackSpeed.ToString())}");
            lines.Add($"{Label("Magic Damage")}{Val($"{stats.minMagicDamage}-{stats.maxMagicDamage}")}");
            lines.Add($"{Label("Magic Resist")}{Val(stats.magicResistance.ToString())}");
            lines.Add($"{Label("Crit Chance")}{Val($"{stats.criticalChance * 100f:F1}%")}");
            lines.Add($"{Label("Stun Chance")}{Val($"{stats.stunChance * 100f:F1}%")}");

            return lines;
        }

        public void ForceCloseChat()
        {
            if (!isChatOpen) return;

            isChatOpen = false;
            chatWasManuallyOpened = false;

            var cw = GUIWindowsManager.instance?.chatWindow;

            cw?.Hide();
            cw?.RemoveFocus();

            cw?.overlayController?.ResumeFadeForAllVisible();
            // Debug.Log("[ForceCloseChat] Chat closed, fade resumed");

            gameplayMap?.Enable();
            guiMap?.Enable();
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
                                "/money, /drop, /tp, /wp, /listwp, /whereami, /summon, /heal, /kill, /buff, /addexp, /lvlup, /repairall, /stats, /godmode, /pause, /resume, /time, /weather, /gremove,/dda, /quests, /zones, /achievements, /info, /clear\n" +                                
                                "Type /help [command] for more details.",
                                GameColors.Gray,
                                italic: true
                            );

                            AddSystemMessage(message);
                        }
                        break;
                    }

                case "info":
                    {
                        var g = Game.instance;
                        if (g != null)
                        {
                            string message = $"{g.gameName} - v.{g.version} Build {g.buildDate}";
                            AddSystemMessage(StringUtils.StringWithColor(message, GameColors.Gray));
                        }
                        else
                        {
                            AddSystemMessage(StringUtils.StringWithColor("Game instance not found.", GameColors.Crimson));
                        }
                        break;
                    }

                case "money":
                    {
                        if (parts.Length < 2)
                        {
                            AddSystemMessage(StringUtils.StringWithColor("Usage: /money <amount> [unit]", GameColors.Gray));
                            break;
                        }

                        int totalAmberlings = 0;

                        for (int i = 1; i < parts.Length; i++)
                        {
                            string part = parts[i].Trim().ToLower();

                            if (int.TryParse(part, out int value))
                            {
                                totalAmberlings += value;
                                continue;
                            }

                            var matchShort = Regex.Match(part, @"^(\d+)([a-z]+)$");
                            if (matchShort.Success)
                            {
                                int amount = int.Parse(matchShort.Groups[1].Value);
                                string unit = matchShort.Groups[2].Value;

                                CurrencyType? type = Currency.ParseUnit(unit);
                                if (type.HasValue)
                                {
                                    totalAmberlings += Currency.ConvertToAmberlings(amount, type.Value);
                                }
                                else
                                {
                                    AddSystemMessage(StringUtils.StringWithColor($"Unknown currency unit: {unit}", GameColors.Crimson));
                                }

                                continue;
                            }

                            if (i + 1 < parts.Length && int.TryParse(part, out int val) && Currency.ParseUnit(parts[i + 1]) is CurrencyType unitType)
                            {
                                totalAmberlings += Currency.ConvertToAmberlings(val, unitType);
                                i++;
                                continue;
                            }

                            AddSystemMessage(StringUtils.StringWithColor($"Invalid input: {parts[i]}", GameColors.Crimson));
                        }

                        if (totalAmberlings > 0)
                        {
                            inventory.AddGold(totalAmberlings);
                            string formatted = Currency.FormatCurrencyString(totalAmberlings);
                            AddSystemMessage(StringUtils.StringWithColorAndStyle($"Get Rich Quick Scheme: {formatted} were given.", GameColors.Gold, bold: true));
                        }
                        else
                        {
                            AddSystemMessage(StringUtils.StringWithColor("No valid currency amount found.", GameColors.Gray));
                        }

                        break;
                    }

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

                case "repairall":
                    {
                        var inv = character.inventory;
                        var ent = character.Entity;

                        if (inv != null)
                        {
                            foreach (var item in inv.currentItems.Keys)
                                item.Repair();
                        }

                        if (ent != null)
                        {
                            foreach (var eq in ent.items.GetEquippedItems())
                                eq?.Repair();

                            AddSystemMessage(StringUtils.StringWithColorAndStyle(
                                "All equipment repaired.", GameColors.Lime));
                        }
                        else
                        {
                            AddSystemMessage(StringUtils.StringWithColor(
                                "Failed to repair: character entity not found.", GameColors.Crimson));
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

                case "buff":
                    {
                        if (parts.Length >= 2 && int.TryParse(parts[1], out int buffId))
                        {
                            var buff = GameDatabase.instance.FindElementById<Buff>(buffId);
                            if (buff != null)
                            {
                                var ent = character.Entity;
                                if (ent != null)
                                {
                                    var manager = ent.GetComponent<EntityBuffManager>();
                                    if (manager != null)
                                    {
                                        if (manager.AddBuff(buff))
                                        {
                                            AddSystemMessage(StringUtils.StringWithColorAndStyle($"Buff {buff.name} applied.", GameColors.Lime));
                                        }
                                        else
                                        {
                                            AddSystemMessage(StringUtils.StringWithColor($"Failed to apply buff {buff.name}.", GameColors.Crimson));
                                        }
                                    }
                                    else
                                    {
                                        AddSystemMessage(StringUtils.StringWithColor("Failed to apply buff: buff manager not found.", GameColors.Crimson));
                                    }
                                }
                                else
                                {
                                    AddSystemMessage(StringUtils.StringWithColor("Failed to apply buff: character entity not found.", GameColors.Crimson));
                                }
                            }
                            else
                            {
                                AddSystemMessage(StringUtils.StringWithColor($"Buff with ID {buffId} not found.", GameColors.Crimson));
                            }
                        }
                        else
                        {
                            AddSystemMessage(StringUtils.StringWithColor("Usage: /buff <id>", GameColors.Gray));
                        }

                        break;
                    }

                case "pause":
                    GamePause.instance.Pause(true);
                    break;

                case "resume":
                    GamePause.instance.Pause(false);
                    break;

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

                        messageHistory.Clear();
                        cw?.ClearOverlayLog();

                        string msg = StringUtils.StringWithColor("Chat history cleared.", GameColors.Gray);
                        cw?.AddOverlayMessage(msg);

                        if (cw?.overlayController != null)
                        {
                            cw.StartCoroutine(RemoveAfterDelay(msg, 5f));
                        }

                        break;
                    }
                    
                case "invite":
                    {
                        if (parts.Length >= 2)
                        {
                            bool success = PartyManager.instance?.Invite(parts[1], false) ?? false;
                            string message = success ?
                                $"Party invite sent to {parts[1]}." :
                                $"Party invite to {parts[1]} blocked by settings.";
                            AddSystemMessage(StringUtils.StringWithColor(message, GameColors.Gray));
                        }
                        else
                        {
                            AddSystemMessage(StringUtils.StringWithColor("Usage: /invite [player]", GameColors.Gray));
                        }
                        break;
                    }

                case "trade":
                    {
                        if (parts.Length >= 2)
                        {
                            TradeManager.instance?.RequestTrade(parts[1]);
                            string message = $"Trade request sent to {parts[1]}.";
                            if (GameSettings.instance != null && GameSettings.instance.GetTradeConfirmations())
                                message += " Awaiting confirmation.";
                            AddSystemMessage(StringUtils.StringWithColor(message, GameColors.Gray));
                        }
                        else
                        {
                            AddSystemMessage(StringUtils.StringWithColor("Usage: /trade [player]", GameColors.Gray));
                        }
                        break;
                    }

                case "drop":
                {
                    int level = 0;
                    int attributes = 0;
                    int elements = 0;
                    int durabilityPercent = 100;
                    int hasSkill = 0;
                    int stackAmount = 1;

                    if (parts.Length >= 3 &&
                        int.TryParse(parts[1], out int group) &&
                        int.TryParse(parts[2], out int idInGroup))
                    {
                        if (parts.Length >= 4) int.TryParse(parts[3], out level);
                        if (parts.Length >= 5) int.TryParse(parts[4], out attributes);
                        if (parts.Length >= 6) int.TryParse(parts[5], out elements);
                        if (parts.Length >= 7) int.TryParse(parts[6], out durabilityPercent);
                        if (parts.Length >= 8) int.TryParse(parts[7], out hasSkill);
                        if (parts.Length >= 9) int.TryParse(parts[8], out stackAmount);

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

                            bool generateElements = elements > 0 && (item is ItemArmor || item is ItemShield);

                            var instance = new ItemInstance(
                                item,
                                attributes > 0,
                                generateElements,
                                attributes, attributes,
                                generateElements ? elements : 0,
                                generateElements ? elements : 0
                            );

                            instance.ForceStack(actualStack);

                            level = Mathf.Clamp(level, 0, 25);
                            for (int j = 0; j < level; j++)
                                instance.UpgradeLevel();

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

                            if (hasSkill == 1 && item is ItemWeapon weapon && GameDatabase.instance.skills.Count > 0)
                            {
                                weapon.skill = GameDatabase.instance.skills[0];
                            }

                            GUI.instance.DropItem(instance);
                        }

                        AddSystemMessage(StringUtils.StringWithColorAndStyle(
                            $"Dropped {item.GetName()} (+{level}, {attributes} attr, {elements} elem, {durabilityPercent}% durability, skill {hasSkill}, stack {stackAmount})",
                            GameColors.Orange));
                    }
                    else
                    {
                        AddSystemMessage(StringUtils.StringWithColor(
                            "Usage: /drop [group] [idInGroup] [level] [attributes] [elements] [durability%] [hasSkill] [stack]",
                            GameColors.Gray));
                    }
                    break;
                }

                case "tp":
                    {
                        if (parts.Length == 4)
                        {
                            bool TryParseCoordinate(string input, out float value)
                            {
                                input = input.Trim().ToUpperInvariant();

                                if (input.EndsWith("N") || input.EndsWith("E") || input.EndsWith("M"))
                                {
                                    return float.TryParse(input[..^1], out value);
                                }

                                if (input.EndsWith("S") || input.EndsWith("W"))
                                {
                                    if (float.TryParse(input[..^1], out value))
                                    {
                                        value = -Mathf.Abs(value);
                                        return true;
                                    }
                                }

                                return float.TryParse(input, out value);
                            }

                            if (TryParseCoordinate(parts[1], out float z) &&
                                TryParseCoordinate(parts[2], out float x) &&
                                TryParseCoordinate(parts[3], out float y))
                            {
                                Vector3 targetPos = new Vector3(x, y, z);

                                var player = Level.instance?.player;

                                if (player == null)
                                {
                                    Debug.LogError("[Teleport System] Player is null!");
                                    AddSystemMessage(StringUtils.StringWithColor("Teleport failed: Player not found.", GameColors.Crimson));
                                    break;
                                }

                                Vector3 before = player.transform.position;

                                var cc = player.GetComponent<CharacterController>();
                                if (cc != null)
                                {
                                    cc.enabled = false;
                                    player.transform.position = targetPos;
                                    cc.enabled = true;
                                }
                                else
                                {
                                    var agent = player.GetComponent<UnityEngine.AI.NavMeshAgent>();
                                    if (agent != null)
                                    {
                                        agent.Warp(targetPos);
                                    }
                                    else
                                    {
                                        player.transform.position = targetPos;
                                    }
                                }

                                Vector3 after = player.transform.position;

                                var zone = AI_DDA.Assets.Scripts.ZoneTrigger.GetCurrentRegion(targetPos);
                                string zoneName = zone != null ? zone.zoneName : "Unknown";

                                string ns = z >= 0 ? $"{Mathf.RoundToInt(z)}N" : $"{-Mathf.RoundToInt(z)}S";
                                string ew = x >= 0 ? $"{Mathf.RoundToInt(x)}E" : $"{-Mathf.RoundToInt(x)}W";
                                string h = $"{Mathf.RoundToInt(y)}m";

                                AddSystemMessage(StringUtils.StringWithColorAndStyle(
                                    $"Teleported to {zoneName} {ns}, {ew}, {h}", GameColors.LightBlue, italic: true));
                            }
                            else
                            {
                                Debug.LogWarning("[Teleport System] Could not parse all coordinates.");
                                AddSystemMessage(StringUtils.StringWithColor("Invalid coordinates. Try /tp 000 000 000 or /tp 000N 000E 000m", GameColors.Gray));
                            }
                        }
                        else
                        {
                            AddSystemMessage(StringUtils.StringWithColor("Usage: /tp <Z> <X> <Y>", GameColors.Gray));
                        }
                        break;
                    }

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

                    AddSystemMessage(StringUtils.StringWithColor($"List of available waypoints:", GameColors.Cyan));

                    for (int i = 0; i < waypoints.Count; i++)
                    {
                        var wp = waypoints[i];
                        lines.Add(StringUtils.StringWithColor($"[{i}] - {wp.title}", GameColors.Gray));
                    }

                    AddSystemMessageBatch(lines);
                    break;

                case "whereami":
                    {
                        var pos = Level.instance.player.transform.position;
                        var zone = AI_DDA.Assets.Scripts.ZoneTrigger.GetCurrentRegion(pos);
                        string zoneName = zone != null ? zone.zoneName : "Wilderness";

                        int z = Mathf.RoundToInt(pos.z);
                        int x = Mathf.RoundToInt(pos.x);
                        int y = Mathf.RoundToInt(pos.y);

                        string ns = z >= 0 ? $"{z}N" : $"{-z}S";
                        string ew = x >= 0 ? $"{x}E" : $"{-x}W";
                        string h = $"{y}m";

                        string formatted =
                            StringUtils.StringWithColor("You are at: ", GameColors.White) +
                            StringUtils.StringWithColor(zoneName, GameColors.Cyan) +
                            StringUtils.StringWithColor(" | (", GameColors.White) +
                            StringUtils.StringWithColor(ns, GameColors.Gold) +
                            StringUtils.StringWithColor(", ", GameColors.White) +
                            StringUtils.StringWithColor(ew, GameColors.Gold) +
                            StringUtils.StringWithColor(", ", GameColors.White) +
                            StringUtils.StringWithColor(h, GameColors.Gold) +
                            StringUtils.StringWithColor(")", GameColors.White);

                        AddSystemMessage(formatted);
                        break;
                    }

                case "summon":
                    {
                        if (parts.Length >= 2 && int.TryParse(parts[1], out int enemyId))
                        {
                            var enemies = GameDatabase.instance.enemies;
                            if (enemyId >= 0 && enemyId < enemies.Count)
                            {
                                var prefab = enemies[enemyId];
                                var position = Level.instance.player.transform.position + Vector3.forward * 2f;
                                GameObject.Instantiate(prefab, position, Quaternion.identity);

                                string enemyName = prefab.name.Replace("(Clone)", "").Trim();

                                string message = StringUtils.StringWithColor("Summoned enemy: ", GameColors.White) +
                                                StringUtils.StringWithColor(enemyName, GameColors.Orange);

                                AddSystemMessage(message);
                            }
                            else
                            {
                                AddSystemMessage(StringUtils.StringWithColor($"Enemy ID {enemyId} is out of range.", GameColors.Crimson));
                            }
                        }
                        else
                        {
                            AddSystemMessage(StringUtils.StringWithColor("Usage: /summon [enemyId]", GameColors.Gray));
                        }
                        break;
                    }

                case "achievements":
                    {
                        var pbLogger = PlayerBehaviorLogger.Instance;

                        if (pbLogger != null && pbLogger.unlockedAchievements.Count > 0)
                        {
                            AddSystemMessage(StringUtils.StringWithColor($"Unlocked Achievements ({pbLogger.unlockedAchievements.Count}):", GameColors.Cyan));
                            foreach (var a in pbLogger.unlockedAchievements)
                                AddSystemMessage(StringUtils.StringWithColor($"• {a}", GameColors.White));
                        }
                        else
                        {
                            AddSystemMessage(StringUtils.StringWithColor("No achievements unlocked yet.", GameColors.Gray));
                        }
                        break;
                    }

                case "quests":
                    {
                        var questData = Game.instance.currentCharacter?.quests;

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
                                string header = $"Completed Quests ({completed.Count}):";
                                AddSystemMessage(StringUtils.StringWithColor(header, GameColors.Cyan));

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
                    }

                case "zones":
                    {
                        var zones = Game.instance.currentCharacter?.visitedZones;

                        if (zones != null && zones.Count > 0)
                        {
                            AddSystemMessage(StringUtils.StringWithColor($"Discovered Zones ({zones.Count}):", GameColors.Cyan));
                            foreach (var zone in zones)
                                AddSystemMessage($"• {zone}");
                        }
                        else
                        {
                            AddSystemMessage(StringUtils.StringWithColor("No zones discovered yet.", GameColors.Gray));
                        }
                        break;
                    }

                case "gremove":
                    {
                        string playerName = character.name;
                        if (!string.IsNullOrEmpty(character.guildName))
                        {
                            string guildName = character.guildName;
                            GuildManager.DeleteGuild();
                            AddSystemMessage($"{playerName} is no longer a member of {guildName} Guild");
                        }
                        else
                        {
                            AddSystemMessage($"{playerName} has no guild assigned");
                        }
                        break;
                    }

                case "stats":
                    {
                        EntityStatsManager statsSource = null;
                        var guiStats = GUIWindowsManager.instance?.stats;

                        if (guiStats != null && guiStats.characterInstance?.Entity != null)
                        {
                            statsSource = guiStats.characterInstance.Entity.stats;
                        }
                        else
                        {
                            statsSource = Game.instance.currentCharacter?.Entity?.stats;
                        }

                        var statslines = BuildStatsLines(statsSource);
                        AddSystemMessageBatch(statslines);
                        break;
                    }

                case "dda":
                    {
                        if (parts.Length == 1)
                        {
                            AddSystemMessage(StringUtils.StringWithColor(
                                "/dda [log/reset/toggle/export/force/type/diff/free]", GameColors.Gray));
                            break;
                        }

                        // ───────── Helpers ─────────
                        string ColorLabel(string label) => StringUtils.StringWithColor($"  {label}: ", GameColors.Gold);
                        string ColorValue(object value) => StringUtils.StringWithColor(value.ToString(), GameColors.White);

                        string sub = parts[1].ToLower();
                        var logger = PlayerBehaviorLogger.Instance;
                        var rlModel = RLModel.Instance;
                        var mlModel = AIModel.Instance;
                        var diffMgr = DifficultyManager.Instance;

                        switch (sub)
                        {
                            case "log":
                                {
                                    if (logger == null)
                                    {
                                        AddSystemMessage(StringUtils.StringWithColor(
                                            "Logger not initialized.", GameColors.Crimson));
                                        break;
                                    }

                                    string playtime = PlayerBehaviorLogger.FormatPlayTime(character.totalPlayTime);

                                    float mlpPrediction = mlModel.PredictDifficulty(
                                        logger.playerDeaths,
                                        logger.enemiesDefeated,
                                        logger.totalCombatTime,
                                        logger.potionsUsed);

                                    float rlFinal = rlModel?.GetCurrentDifficulty() ?? 0f;

                                    var ddalines = new List<string>
                                    {
                                        StringUtils.StringWithColor("[AI-DDA]", GameColors.Cyan),

                                        $"{ColorLabel("Play Time")}{ColorValue(playtime)}",
                                        $"{ColorLabel("Combat Time")}{ColorValue($"{logger.totalCombatTime:F1}s")}",
                                        $"{ColorLabel("Player Deaths")}{ColorValue(logger.playerDeaths)}",
                                        $"{ColorLabel("Enemies Defeated")}{ColorValue(logger.enemiesDefeated)}",
                                        $"{ColorLabel("Potions Used")}{ColorValue(logger.potionsUsed)}",
                                        $"{ColorLabel("Zones Discovered")}{ColorValue(logger.zonesDiscovered)}",
                                        $"{ColorLabel("Quests Completed")}{ColorValue(logger.questsCompleted)}",
                                        $"{ColorLabel("Achievements")}{ColorValue(logger.unlockedAchievements.Count)}",
                                        $"{ColorLabel("Current Bartle's Type")}{ColorValue(logger.currentDynamicPlayerType)}",
                                        $"{ColorLabel("MLP Predicted Difficulty")}{ColorValue($"{mlpPrediction:F2}")}",
                                        $"{ColorLabel("RL Final Difficulty")}{ColorValue($"{rlFinal:F2}")}",
                                        $"{ColorLabel("Manual Override")}{ColorValue(diffMgr?.isManualOverride ?? false)}"
                                    };

                                    AddSystemMessage(string.Join("\n", ddalines));
                                    break;
                                }

                            case "reset":
                                if (logger != null)
                                {
                                    logger.ResetData();
                                    AddSystemMessage(StringUtils.StringWithColor(
                                        "Player behavior stats reset.", GameColors.Orange));
                                }
                                break;

                            case "export":
                                if (logger != null)
                                {
                                    logger.ExportPlayerData();
                                    AddSystemMessage(StringUtils.StringWithColor(
                                        "Exported player data to CSV.", GameColors.Lime));
                                }
                                break;

                            case "toggle":
                                if (diffMgr != null)
                                {
                                    diffMgr.useAIDDA = !diffMgr.useAIDDA;
                                    string state = diffMgr.useAIDDA ? "ON" : "OFF";
                                    AddSystemMessage(StringUtils.StringWithColor($"AI-DDA: {state}", GameColors.Gold));
                                }
                                break;

                            case "force":
                                {
                                    if (rlModel == null)
                                    {
                                        AddSystemMessage(StringUtils.StringWithColor(
                                            "RLModel not initialized.", GameColors.Crimson));
                                        break;
                                    }

                                    float cur = rlModel.GetCurrentDifficulty();
                                    rlModel.AdjustDifficulty(cur);

                                    AddSystemMessage(StringUtils.StringWithColorAndStyle(
                                        $"[AI-DDA] Requested difficulty recalculation (base {cur:F2})",
                                        GameColors.LightBlue));
                                    break;
                                }

                            case "type":
                                if (logger != null)
                                {
                                    string msg =
                                        StringUtils.StringWithColor("Dynamic player Bartle's type: ", GameColors.White) +
                                        StringUtils.StringWithColor(logger.currentDynamicPlayerType.ToString(), GameColors.Cyan);
                                    AddSystemMessage(msg);
                                }
                                else
                                {
                                    AddSystemMessage(StringUtils.StringWithColor(
                                        "Logger not initialized.", GameColors.Crimson));
                                }
                                break;

                            case "diff":
                                {
                                    if (parts.Length < 3 || !float.TryParse(parts[2], out float val))
                                    {
                                        AddSystemMessage(StringUtils.StringWithColor(
                                            "Usage: /dda diff [value]", GameColors.Gray));
                                        break;
                                    }

                                    if (diffMgr == null || rlModel == null)
                                    {
                                        AddSystemMessage(StringUtils.StringWithColor(
                                            "Difficulty system not initialized.", GameColors.Crimson));
                                        break;
                                    }

                                    diffMgr.ManualSetDifficulty(val);
                                    rlModel.SetCurrentDifficulty(val);

                                    AddSystemMessage(StringUtils.StringWithColor(
                                        $"[AI-DDA] Manual difficulty override = {val:F2}.  Use /dda free to release.",
                                        GameColors.Orange));
                                    break;
                                }

                            case "free":
                                if (diffMgr != null)
                                {
                                    diffMgr.ClearManualOverride();
                                    AddSystemMessage(StringUtils.StringWithColor(
                                        "[AI-DDA] Manual override disabled – AI back in control.",
                                        GameColors.LightBlue));
                                }
                                break;

                            default:
                                AddSystemMessage(StringUtils.StringWithColor(
                                    $"Unknown /dda command: {sub}", GameColors.Crimson));
                                break;
                        }

                        break;
                    }
            }
        }

        private IEnumerator RemoveAfterDelay(string message, float delay)
        {
            yield return new WaitForSeconds(delay);

            var cw = GUIWindowsManager.instance?.chatWindow;
            if (cw == null || cw.overlayLogContent == null) yield break;

            foreach (Transform child in cw.overlayLogContent)
            {
                var text = child.GetComponentInChildren<UnityEngine.UI.Text>();
                if (text != null && text.text == message)
                {
                    cw.overlayController?.FadeMessage(child.gameObject);
                    break;
                }
            }
        }
    }
}
