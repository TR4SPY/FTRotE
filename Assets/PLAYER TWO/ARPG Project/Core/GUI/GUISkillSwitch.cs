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

    // Referencja do wygenerowanej klasy z pliku .inputactions
    private EntityControls controls;

    // Flaga, który panel jest wyświetlony
    private bool showingFirstSet = true;

    private void Awake()
    {
        // Inicjalizujemy akcje
        controls = new EntityControls();
    }

    private void OnEnable()
    {
        // Uruchamiamy mapę "Gameplay" (nazwa musi się zgadzać z Twoim plikiem)
        controls.Gameplay.Enable();

        // Subskrypcja pod akcje "Select Skill X"
        controls.Gameplay.SelectSkill0.performed += ctx => OnSkillPressed(0);
        controls.Gameplay.SelectSkill1.performed += ctx => OnSkillPressed(1);
        controls.Gameplay.SelectSkill2.performed += ctx => OnSkillPressed(2);
        controls.Gameplay.SelectSkill3.performed += ctx => OnSkillPressed(3);
        controls.Gameplay.SelectSkill4.performed += ctx => OnSkillPressed(4);
        controls.Gameplay.SelectSkill5.performed += ctx => OnSkillPressed(5);
        controls.Gameplay.SelectSkill6.performed += ctx => OnSkillPressed(6);
        controls.Gameplay.SelectSkill7.performed += ctx => OnSkillPressed(7);
    }

    private void OnDisable()
    {
        // Dobrze jest odsubskrybować, żeby uniknąć „wycieków” eventów
        controls.Gameplay.SelectSkill0.performed -= ctx => OnSkillPressed(0);
        controls.Gameplay.SelectSkill1.performed -= ctx => OnSkillPressed(1);
        controls.Gameplay.SelectSkill2.performed -= ctx => OnSkillPressed(2);
        controls.Gameplay.SelectSkill3.performed -= ctx => OnSkillPressed(3);
        controls.Gameplay.SelectSkill4.performed -= ctx => OnSkillPressed(4);
        controls.Gameplay.SelectSkill5.performed -= ctx => OnSkillPressed(5);
        controls.Gameplay.SelectSkill6.performed -= ctx => OnSkillPressed(6);
        controls.Gameplay.SelectSkill7.performed -= ctx => OnSkillPressed(7);

        controls.Gameplay.Disable();
    }

    private void Start()
    {
        // Na starcie włączamy panel 1–4, wyłączamy 5–8
        ShowFirstSet();
    }

    /// <summary>
    /// Wywoływane, gdy któryś z klawiszy 1..8 zostanie wciśnięty.
    /// </summary>
    private void OnSkillPressed(int skillIndex)
    {
        // 0..3 → klawisze 1..4
        // 4..7 → klawisze 5..8
        if (skillIndex <= 3 && !showingFirstSet)
        {
            ShowFirstSet();
        }
        else if (skillIndex >= 4 && showingFirstSet)
        {
            ShowSecondSet();
        }

        // Tu wywołaj wybór skilla w systemie
        // Zakładam, że m_entity.skills.equipped[skillIndex] jest skillami 1..8
        SelectSkill(skillIndex);
    }

    private void SelectSkill(int index)
    {
        // Tutaj wywołujesz logikę wybierania skilla,
        // np. w oparciu o GUIEntity (jak w Twoich plikach).
        // Najczęściej: 
        if (GUIEntity.instance && GUIEntity.instance.m_entity)
        {
            // Wywołaj ChangeTo() na skillu z equpped o danym indeksie
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
