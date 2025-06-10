using UnityEngine;
using UnityEngine.InputSystem;
using PLAYERTWO.ARPGProject;
using UnityEngine.UI;

public class GUISkillSwitch : MonoBehaviour
{
    [Header("Panele na HUD")]
    [Tooltip("Panel z slotami na skille 1–4.")]
    public GameObject skillsPanel1;
    [Tooltip("Panel z slotami na skille 5–8.")]
    public GameObject skillsPanel2;
    [Tooltip("Current Skill set text")]
    public Text SkillSwitchText;

    private InputActionMap gameplayMap;
    private readonly InputAction[] skillActions = new InputAction[8];
    private readonly System.Action<InputAction.CallbackContext>[] skillCallbacks = new System.Action<InputAction.CallbackContext>[8];
    private bool showingFirstSet = true;

    private void Awake()
    {
        gameplayMap = Game.instance.gameplayActions?.FindActionMap("Gameplay");

        if (gameplayMap == null)
        {
            Debug.LogError("[GUISkillSwitch] Gameplay input action map not found.");
            return;
        }

        for (int i = 0; i < 8; i++)
        {
            var action = gameplayMap.FindAction($"Select Skill {i}");
            if (action != null)
            {
                skillActions[i] = action;
                int index = i;
                skillCallbacks[i] = ctx => OnSkillPressed(index);
            }
            else
            {
                Debug.LogWarning($"[GUISkillSwitch] Action 'Select Skill {i}' not found in Gameplay map.");
            }
        }
    }

    private void OnEnable()
    {
        gameplayMap?.Enable();

        for (int i = 0; i < 8; i++)
        {
            if (skillActions[i] != null && skillCallbacks[i] != null)
                skillActions[i].performed += skillCallbacks[i];
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < 8; i++)
        {
            if (skillActions[i] != null && skillCallbacks[i] != null)
                skillActions[i].performed -= skillCallbacks[i];
        }

        gameplayMap?.Disable();
    }

    private void Start()
    {
        ShowFirstSet();
    }

    /// <summary>
    /// Wywoływane, gdy któryś z klawiszy 1..8 zostanie wciśnięty.
    /// </summary>
    private void OnSkillPressed(int skillIndex)
    {
        if (skillIndex <= 3 && !showingFirstSet)
        {
            ShowFirstSet();
        }
        else if (skillIndex >= 4 && showingFirstSet)
        {
            ShowSecondSet();
        }

        SelectSkill(skillIndex);
    }

    private void SelectSkill(int index)
    {
        if (GUIEntity.instance && GUIEntity.instance.m_entity)
        {
            var skills = GUIEntity.instance.m_entity.skills;
            if (index < skills.equipped.Count && skills.equipped[index] != null)
            {
                skills.ChangeTo(skills.equipped[index].data);
            }
        }
    }

    private void ShowFirstSet()
    {
        CanvasGroup g1 = skillsPanel1.GetComponent<CanvasGroup>();
        CanvasGroup g2 = skillsPanel2.GetComponent<CanvasGroup>();

        if (g1)
        {
            g1.alpha = 1f;
            g1.interactable = true;
            g1.blocksRaycasts = true;
        }
        if (g2)
        {
            g2.alpha = 0f;
            g2.interactable = false;
            g2.blocksRaycasts = false;
        }

        showingFirstSet = true;
        SkillSwitchText.text = "1";
    }

    private void ShowSecondSet()
    {
        CanvasGroup g1 = skillsPanel1.GetComponent<CanvasGroup>();
        CanvasGroup g2 = skillsPanel2.GetComponent<CanvasGroup>();

        if (g1)
        {
            g1.alpha = 0f;
            g1.interactable = false;
            g1.blocksRaycasts = false;
        }
        if (g2)
        {
            g2.alpha = 1f;
            g2.interactable = true;
            g2.blocksRaycasts = true;
        }

        showingFirstSet = false;
        SkillSwitchText.text = "2";
    }
}
