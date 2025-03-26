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
            gameObject.SetActive(true);
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
            List<Dialog.DialogOption> availableOptions = currentDialog.GetFilteredOptions(currentPageIndex, playerType);

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

        private void ExecuteOption(Dialog.DialogOption option)
        {
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
                case Dialog.DialogAction.SetSpecialCondition:
                    Game.instance.currentCharacter.SetSpecialCondition(option.specialConditionToSet);
                    Debug.Log($"[AI-DDA] Gracz wybrał ścieżkę: {option.specialConditionToSet}");
                    ContinueDialog(option.nextPageIndex);
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

            questGiver.OpenQuestDialog();
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

        private void OpenBlacksmithWindow()
        {
            if (!(currentNPC is Blacksmith blacksmith))
            {
                Debug.LogError("[AI-DDA] Błąd: currentNPC nie jest Blacksmith!");
                return;
            }
            
            blacksmith.OpenBlackSmithService(player);
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

        private void SetPlayerTypeIcon(Dialog.DialogOption option, Image iconImage)
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

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}
