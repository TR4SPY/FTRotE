using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using PLAYERTWO.ARPGProject;
using Unity.VisualScripting;

namespace AI_DDA.Assets.Scripts
{
    public class GUIDialogWindow : MonoBehaviour
    {
        public Text dialogTitle;
        public Text dialogText;
        public Button[] optionButtons; 

        private Dialog currentDialog;
        private int currentPageIndex = 0;
        private Entity player;
        private Interactive currentNPC;
        public Sprite[] playerTypeSprites; 

        public void Show(Entity player, Interactive npc, Dialog dialog)
        {
            var rect = GetComponent<RectTransform>();
            
            gameObject.SetActive(true);
            rect.SetAsLastSibling();
            this.player = player;
            currentNPC = npc;
            currentDialog = dialog;
            string npcID = npc.GetNPCID();

            if (Game.instance.currentCharacter.viewedDialogPages.Count == 0) 
            {
                currentDialog.ResetPathChoices();
            }

            if (int.TryParse(npcID, out int npcIntID))
            {
                if (!Game.instance.currentCharacter.selectedDialogPaths.ContainsKey(npcIntID))
                {
                    currentDialog.ResetPathChoices();
                }
            }

            int rememberedPath = currentDialog.GetLastPathChoice();

            Debug.Log($"[Show] rememberedPath = {rememberedPath}");

            if (rememberedPath != -1)
            {
                currentPageIndex = rememberedPath;
            }
            else
            {
                currentPageIndex = 0;
                while (currentPageIndex < currentDialog.pages.Count &&
                    !currentDialog.ShouldShowPage(npcID, currentPageIndex))
                {
                    currentPageIndex++;
                }
            }

            SetupDialog();
        }

        private void SetupDialog()
        {
            dialogTitle.text = currentDialog.dialogTitle;

            if (currentDialog.pages.Count == 0) return;

            var currentPage = currentDialog.pages[currentPageIndex];

            string playerSpecialCondition = Game.instance.currentCharacter.GetSpecialConditionAsString();
            if (currentPage.isSpecial && currentPage.specialCondition != playerSpecialCondition)
            {
                ContinueDialog();
                return;
            }

            dialogText.text = currentPage.dialogText;

            string playerType = Game.instance.currentCharacter.currentDynamicPlayerType;
            var questGiver = currentNPC?.GetComponent<QuestGiver>();
            List<DialogOption> availableOptions = currentDialog.GetFilteredOptions(currentPageIndex, playerType, questGiver);

            for (int i = 0; i < optionButtons.Length; i++)
            {
                optionButtons[i].gameObject.SetActive(false);
                optionButtons[i].onClick.RemoveAllListeners();
            }

            for (int i = 0; i < availableOptions.Count && i < optionButtons.Length; i++)
            {
                var option = availableOptions[i];

                optionButtons[i].gameObject.SetActive(true);
                optionButtons[i].GetComponentInChildren<Text>().text = option.optionText;
                optionButtons[i].onClick.AddListener(() => ExecuteOption(option));

                Image playerTypeIcon = optionButtons[i].transform.Find("PlayerTypeIcon")?.GetComponent<Image>();

                if (playerTypeIcon != null)
                {
                    SetPlayerTypeIcon(option, playerTypeIcon);
                }
            }
        }

        private void ExecuteOption(DialogOption option)
        {
            var character = Game.instance.currentCharacter;
            if (option.nextPageIndex != -1 && character != null)
            {
                character.SetDialogPathChoice(currentPageIndex, option.nextPageIndex);
            }

            switch (option.action)
            {
                case Dialog.DialogAction.OpenQuests:
                    OpenQuestWindow();
                    break;

                case Dialog.DialogAction.OpenShop:
                    OpenMerchantWindow();
                    break;

                case Dialog.DialogAction.OpenCrafting:
                    OpenCraftmanWindow();
                    break;
                    
                case Dialog.DialogAction.OpenAlchemyWindow:
                    OpenAlchemyWindow();
                    break;

                case Dialog.DialogAction.OpenBlacksmith:
                    OpenBlacksmithWindow();
                    break;

                case Dialog.DialogAction.ContinueDialog:
                    if (option.nextPageIndex != -1)
                    {
                        currentDialog.SetPathChoice(currentPageIndex, option.nextPageIndex);
                    }
                    ContinueDialog(option.nextPageIndex);
                    break;

                case Dialog.DialogAction.Exclusive:
                    OpenExclusiveWindow();
                    break;

                case Dialog.DialogAction.OpenClassUpgrade:
                    OpenClassUpgradeWindow();
                    break;

                case Dialog.DialogAction.SetSpecialCondition:
                   // var character = Game.instance.currentCharacter;
                    character.SetSpecialCondition(option.specialConditionToSet);

                    var entity = character.Entity;
                    if (entity != null && entity.nametag != null)
                    {
                        string playerName = character.name;
                        int playerLevel = character.stats.currentLevel;
                        string classDisplay = character.GetName();
                        string guild = character.guildName;

                        entity.nametag.SetNametag(
                            playerName: playerName,
                            level: playerLevel,
                            guild: guild,
                            className: classDisplay
                        );
                    }

                    ContinueDialog(option.nextPageIndex);
                    break;

                case Dialog.DialogAction.OpenGuildmaster:
                    OpenGuildmasterWindow();
                    break;

                case Dialog.DialogAction.GiveBuff:
                case Dialog.DialogAction.GiveDebuff:
                    var buffManager = player.GetComponent<EntityBuffManager>();
                    if (buffManager != null && option.buff != null)
                    {
                        bool isDebuff = option.action == Dialog.DialogAction.GiveDebuff;
                        bool added = buffManager.AddBuff(option.buff, isDebuff);
                        if (!added)
                        {
                            Debug.LogWarning($"[GUIDialogWindow] Failed to add {(isDebuff ? "debuff" : "buff")} '{option.buff.name}'.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[GUIDialogWindow] Buff manager or buff is missing for dialog option.");
                    }
                    ContinueDialog(option.nextPageIndex);
                    break;

                case Dialog.DialogAction.OpenBank:
                    OpenBankWindow();
                    break;
                
                case Dialog.DialogAction.OpenSpecialization:
                    OpenSpecializationWindow();
                    break;

                default:
                    Close();
                    break;
            }
        }

        private void ContinueDialog(int targetPageIndex = -1)
        {
            if (currentDialog == null) return;

            string npcID = currentNPC.GetNPCID();
            Game.instance.currentCharacter.MarkDialogPageAsViewed(npcID, currentPageIndex);

            int rememberedPath = currentDialog.GetNextPageFromPath(currentPageIndex);
            if (rememberedPath != -1)
            {
                targetPageIndex = rememberedPath;
            }

            if (targetPageIndex == -1)
            {
                if (currentPageIndex + 1 >= currentDialog.pages.Count)
                {
                    Close();
                    return;
                }
                targetPageIndex = currentPageIndex + 1;
            }

            currentDialog.SetPathChoice(currentPageIndex, targetPageIndex);
            var character = Game.instance.currentCharacter;
            if (character != null)
            {
                character.SetDialogPathChoice(currentPageIndex, targetPageIndex);
            }
            currentPageIndex = targetPageIndex;

            while (!currentDialog.ShouldShowPage(npcID, currentPageIndex))
            {
                int nextPath = currentDialog.GetNextPageFromPath(currentPageIndex);
                if (nextPath != -1)
                {
                    currentPageIndex = nextPath;
                }
                else
                {
                    currentPageIndex++;
                }

                if (currentPageIndex >= currentDialog.pages.Count)
                {
                    Close();
                    return;
                }
            }

            SetupDialog();
        }

        public void OpenQuestWindow()
        {
            var questGiver = currentNPC.GetComponent<QuestGiver>();

            if (questGiver == null)
            {
                Debug.LogError("[AI-DDA] Błąd: currentNPC nie ma komponentu QuestGiver!");
                return;
            }

            questGiver.OpenQuestDialog(currentPageIndex);
        }

        private void OpenMerchantWindow()
        {
            if (!(currentNPC is Merchant merchant))
            {
                Debug.LogError("[AI-DDA] Błąd: currentNPC nie jest Merchantem!");
                return;
            }

            merchant.OpenMerchantShop();
        }

        private void OpenCraftmanWindow()
        {
            if (!(currentNPC is Craftman craftman))
            {
                Debug.LogError("[AI-DDA] Błąd: currentNPC nie jest Craftmanem!");
                return;
            }

            craftman.OpenCraftingService();
        }

        private void OpenAlchemyWindow()
        {
            if (!(currentNPC is Alchemist alchemist))
            {
                Debug.LogError("[AI-DDA] Błąd: currentNPC nie jest Alchemistem!");
                return;
            }

            alchemist.OpenAlchemyService();
        }

        private void OpenGuildmasterWindow()
        {
            if (!(currentNPC is Guildmaster guildmaster))
            {
                Debug.LogError("[AI-DDA] Błąd: currentNPC nie jest Guildmasterem!");
                return;
            }

            guildmaster.OpenGuildmasterService();
        }

        private void OpenBlacksmithWindow()
        {
            if (!(currentNPC is Blacksmith blacksmith))
            {
                Debug.LogError("[AI-DDA] Błąd: currentNPC nie jest Blacksmith!");
                return;
            }
            
            blacksmith.OpenBlackSmithService(player);
        }

        private void OpenBankWindow()
        {
            var manager = GUIWindowsManager.instance;
            if (manager == null || manager.bankWindow == null)
            {
                Debug.LogError("[AI-DDA] Błąd: bankWindow nie jest przypisane w GUIWindowsManager!");
                return;
            }

            manager.bankWindow.Show();
        }

        public void OpenExclusiveWindow()
        {
            var questGiver = currentNPC.GetComponent<QuestGiver>();

            if (questGiver == null)
            {
                Debug.LogError("[AI-DDA] Błąd: currentNPC nie ma komponentu QuestGiver!");
                return;
            }

            var pType = Game.instance.currentCharacter.currentDynamicPlayerType;
            var exQuest = questGiver.CurrentExclusiveQuest(pType);

            if (exQuest == null)
            {
                Debug.Log("No exclusive quest for this player type or all completed.");
                return;
            }

            GUIWindowsManager.instance.exclusiveWindow.SetQuest(exQuest);
        }
        public void OpenClassUpgradeWindow()
        {
            var questGiver = currentNPC.GetComponent<QuestGiver>();
            if (questGiver == null)
            {
                Debug.LogError("Brak komponentu QuestGiver.");
                return;
            }

            var upgradeQuest = questGiver.CurrentClassUpgradeQuest();
            if (upgradeQuest == null)
            {
                Debug.Log("Brak questów typu class upgrade.");
                return;
            }

            GUIWindowsManager.instance.exclusiveWindow.SetQuest(upgradeQuest);
        }

        public void OpenSpecializationWindow()
        {
            var questGiver = currentNPC.GetComponent<QuestGiver>();
            if (questGiver == null)
            {
                Debug.LogError("Brak komponentu QuestGiver.");
                return;
            }

            var specQuest = questGiver.CurrentSpecializationQuest();
            if (specQuest == null)
            {
                Debug.Log("Brak questów typu specialization.");
                return;
            }

            GUIWindowsManager.instance.exclusiveWindow.SetQuest(specQuest);
        }

        private void SetPlayerTypeIcon(DialogOption option, Image iconImage)
        {
            iconImage.gameObject.SetActive(false);

            if (option.isForAchiever && playerTypeSprites.Length > 0)
            {
                iconImage.sprite = playerTypeSprites[0]; 
                iconImage.gameObject.SetActive(true);
            }
            else if (option.isForKiller && playerTypeSprites.Length > 1)
            {
                iconImage.sprite = playerTypeSprites[1];
                iconImage.gameObject.SetActive(true);
            }
            else if (option.isForExplorer && playerTypeSprites.Length > 2)
            {
                iconImage.sprite = playerTypeSprites[2];
                iconImage.gameObject.SetActive(true);
            }
            else if (option.isForSocializer && playerTypeSprites.Length > 3)
            {
                iconImage.sprite = playerTypeSprites[3];
                iconImage.gameObject.SetActive(true);
            }
        }

        public void Refresh()
        {
            SetupDialog();
        }
        
        public void SetCurrentPage(int page)
        {
            currentPageIndex = page;
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}
