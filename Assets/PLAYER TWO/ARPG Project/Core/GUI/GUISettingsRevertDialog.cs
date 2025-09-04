using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PLAYERTWO.ARPGProject;

/// <summary>
/// Generic dialog window that asks the player to confirm the recently
/// applied settings. If the player does not confirm within the countdown
/// time the provided rollback action will be executed.
/// </summary>
public class GUISettingsRevertDialog : GUIWindow
{
    [Header("Dialog Components")]
    [Tooltip("Button used to keep the new settings.")]
    public Button applyButton;

    [Tooltip("Button used to rollback to the previous settings.")]
    public Button revertButton;

    [Tooltip("Text element displaying the remaining time to confirm.")]
    public TMP_Text countdownLabel;

    [Tooltip("Default time (in seconds) before the changes are reverted.")]
    public float defaultCountdown = 15f;

    private float m_timeRemaining;
    private Action m_onApply;
    private Action m_onRevert;
    private bool m_countingDown;

    public static GUIApplyRevertDialog instance { get; private set; }

    protected virtual void Awake()
    {
        instance = this;

        if (applyButton != null)
            applyButton.onClick.AddListener(Apply);

        if (revertButton != null)
            revertButton.onClick.AddListener(Revert);
    }

    /// <summary>
    /// Shows the dialog starting a new countdown.
    /// </summary>
    public void Show(Action onApply, Action onRevert, float duration = -1f)
    {
        m_onApply = onApply;
        m_onRevert = onRevert;
        m_timeRemaining = duration > 0f ? duration : defaultCountdown;
        m_countingDown = true;
        UpdateCountdown();
        base.Show();
    }

    private void Apply()
    {
        m_countingDown = false;
        m_onApply?.Invoke();
        Hide();
    }

    private void Revert()
    {
        m_countingDown = false;
        m_onRevert?.Invoke();
        Hide();
    }

    protected virtual void Update()
    {
        if (!m_countingDown) return;

        m_timeRemaining -= Time.unscaledDeltaTime;
        if (m_timeRemaining <= 0f)
        {
            Revert();
        }
        else
        {
            UpdateCountdown();
        }
    }

    private void UpdateCountdown()
    {
        if (countdownLabel != null)
            countdownLabel.text = Mathf.CeilToInt(m_timeRemaining).ToString();
    }
}
