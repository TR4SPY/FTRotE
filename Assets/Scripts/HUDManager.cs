using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    private static HUDManager instance;

    public static HUDManager Instance
    {
        get
        {
            if (instance == null)
            {
                #if UNITY_2023_1_OR_NEWER
                instance = Object.FindFirstObjectByType<HUDManager>();
                #else
                instance = Object.FindObjectOfType<HUDManager>();
                #endif

                if (instance == null)
                {
                    instance = new GameObject("HUDManager").AddComponent<HUDManager>();
                    Debug.LogWarning("HUDManager was not found in the scene. A new instance has been created.");
                }
            }
            return instance;
        }
    }

    private Queue<IHUD> hudQueue = new Queue<IHUD>();
    private IHUD activeHUD = null;
    private bool isAnimating = false; // Blokada animacji

    public void RequestDisplay(IHUD hud)
    {
        if (activeHUD == null && !isAnimating) // Sprawdza, czy animacja się zakończyła
        {
            activeHUD = hud;
            isAnimating = true; // Blokada nowych animacji
            activeHUD.Show();
        }
        else
        {
            hudQueue.Enqueue(hud);
        }
    }

    public void OnHUDHidden(IHUD hud)
    {
        if (activeHUD == hud)
        {
            activeHUD = null;
            isAnimating = false; // Odblokowanie po zakończeniu animacji

            if (hudQueue.Count > 0)
            {
                activeHUD = hudQueue.Dequeue();
                isAnimating = true; // Ponownie blokuje przed kolejnym HUD
                activeHUD.Show();
            }
        }
    }

    public interface IHUD
    {
        void Show();
        void Hide();
    }
}
