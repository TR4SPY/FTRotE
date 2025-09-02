using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using PLAYERTWO.ARPGProject;

public class GUILanguageSettingsWindow : GUIWindow
{
    [Header("Language Settings")]
    public TMP_Dropdown languageDropdown;

    [Header("Navigation Buttons")]
    public Button backButton;

    protected GameSettings m_settings => GameSettings.instance;

    protected override void Start()
    {
        base.Start();

        InitializeLanguages();

        if (backButton != null)
        {
            backButton.onClick.AddListener(BackButton);
        }
        else
        {
            Debug.LogError("[Settings] Back Button is NULL! Assign it in the Inspector.");
        }
    }

    protected virtual void InitializeLanguages()
    {
        if (languageDropdown == null)
        {
            Debug.LogError("[Settings] Language Dropdown is NULL! Assign it in the Inspector.");
            return;
        }

        var locales = LocalizationSettings.AvailableLocales.Locales;

        languageDropdown.ClearOptions();
        var options = new List<string>();
        foreach (var locale in locales)
        {
            options.Add(locale.LocaleName);
        }
        languageDropdown.AddOptions(options);

        int current = m_settings.GetLanguage();
        languageDropdown.value = current;
        languageDropdown.onValueChanged.AddListener(idx =>
        {
            m_settings.SetLanguage(idx);
        });
    }

    public void BackButton()
    {
        Hide();
        if (GUIWindowsManager.Instance != null && GUIWindowsManager.Instance.settingsWindow != null)
        {
            GUIWindowsManager.Instance.settingsWindow.ShowMainSettings();
        }
        else
        {
            Debug.LogError("[Settings] settingsWindow is NULL! Make sure it's assigned in GUIWindowsManager.");
        }
    }
}
