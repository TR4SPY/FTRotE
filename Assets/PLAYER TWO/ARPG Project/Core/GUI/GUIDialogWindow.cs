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
            currentPageIndex = 0;
            SetupDialog();
        }

        private void SetupDialog()
        {
            dialogTitle.text = currentDialog.dialogTitle;

            if (currentDialog.pages.Count == 0) return;

            var currentPage = currentDialog.pages[currentPageIndex];
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
                    ContinueDialog();
                    break;
                case Dialog.DialogAction.Exclusive:
                    OpenExclusiveWindow();
                    break;
                default:
                    Close();
                    break;
            }
        }

        private void ContinueDialog()
        {
            if (currentDialog == null || currentPageIndex + 1 >= currentDialog.pages.Count)
            {
                Close();
                return;
            }

            currentPageIndex++;
            SetupDialog();
        }

        private void OpenQuestWindow()
        {
            var nextQuest = Game.instance.quests.GetNextAvailableQuest();
            if (nextQuest != null)
            {
                GUIWindowsManager.instance.quest.SetQuest(nextQuest);
            }
            else
            {
                Debug.Log("[AI-DDA] Brak dostępnych questów!");
            }
        }

        private void OpenMerchantWindow()
        {
            if (!(currentNPC is Merchant merchant))
            {
                Debug.LogError("[AI-DDA] Błąd: currentNPC nie jest Merchantem!");
                return;
            }

            merchant.OpenMerchantShop(); // ✅ Teraz poprawnie wywołujemy metodę na instancji Merchanta
        }

        private void OpenBlacksmithWindow()
        {
            GUIWindowsManager.instance.blacksmith.Show();
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
