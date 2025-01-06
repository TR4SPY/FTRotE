using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Game/Level Audio")]
    public class LevelAudio : MonoBehaviour
    {
        [Tooltip("The Audio Clip that represents the theme sound of this Level.")]
        public AudioClip theme;

        protected GameAudio m_audio => GameAudio.instance;
        
        protected virtual void Start()
        {
            // Sprawdź, czy obecnie grany utwór to już ten sam, co przypisany w tym LevelAudio
            if (m_audio.CurrentMusic != theme)
            {
                m_audio.PlayMusic(theme);
            }
        }

        protected virtual void OnDisable()
        {
            // Nie zatrzymuj muzyki, jeśli przechodzimy do innej sceny
            if (m_audio.CurrentMusic == theme)
            {
                Debug.Log($"Leaving LevelAudio for {theme.name}, but music continues.");
            }
            else
            {
                Debug.Log($"Music stopped.");
            }
        }

        //protected virtual void Start() => m_audio.PlayMusic(theme);
       //protected virtual void OnDisable() => GameAudio.instance?.StopMusic();
    }
}
