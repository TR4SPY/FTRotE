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

        public void Show(Entity player, Interactive npc, Dialog dialog)
        {
            gameObject.SetActive(true);
            this.player = player;
            currentNPC = npc;
            currentDialog = dialog;

            if (Game.instance.currentCharacter.viewedDialogPages.Count == 0) 
            {
                Debug.Log("[GUIDialogWindow] Nowa postać - resetujemy lastPathChoice!");
                currentDialog.ResetPathChoices(); // Trzeba dodać metodę resetującą w `Dialog`
            }

            // Pobranie zapamiętanej ścieżki dialogu
            int rememberedPath = currentDialog.GetLastPathChoice();
            Debug.Log($"[Show] rememberedPath = {rememberedPath}");

            if (rememberedPath != -1)
            {
                currentPageIndex = rememberedPath;
            }
            else
            {
                currentPageIndex = 0;
                while (currentPageIndex < currentDialog.pages.Count && !currentDialog.ShouldShowPage(currentPageIndex))
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
                Debug.Log($"[AI-DDA] Strona {currentPageIndex} nie jest dostępna dla gracza z warunkiem {playerSpecialCondition}");
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

            // currentDialog.MarkPageAsViewed(currentPageIndex);
            Game.instance.currentCharacter.MarkDialogPageAsViewed(currentPageIndex);

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

            while (!currentDialog.ShouldShowPage(currentPageIndex))
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

        private void OpenQuestWindow()
        {
            if (!(currentNPC is QuestGiver questGiver))
            {
                Debug.LogError("[AI-DDA] Błąd: currentNPC nie jest QuestGiverem!");
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

        private void OpenBlacksmithWindow()
        {
            if (!(currentNPC is Blacksmith blacksmith))
            {
                Debug.LogError("[AI-DDA] Błąd: currentNPC nie jest Blacksmith!");
                return;
            }
            
            blacksmith.OpenBlackSmithService(player);
        }

        private void OpenExclusiveWindow()
        {
            // GUIWindowsManager.instance.exclusive.Show();
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}
