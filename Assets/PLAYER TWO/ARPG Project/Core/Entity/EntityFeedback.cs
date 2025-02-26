using UnityEngine;
using System.Collections;

namespace PLAYERTWO.ARPGProject
{
    [RequireComponent(typeof(Entity))]
    [AddComponentMenu("PLAYER TWO/ARPG Project/Entity/Entity Feedback")]
    public class EntityFeedback : MonoBehaviour
    {
        [Header("Damage Text")]
        [Tooltip("A prefab that shows up when the Entity takes damage.")]
        public GameObject damageText;

        [Tooltip("The position offset applied when the damage text is instantiated.")]
        public Vector3 damageTextOffset = new(0, 1, 0);

        [Header("Damage Particles")]
        [Tooltip("The particle that plays when the Entity takes damage.")]
        public ParticleSystem damageParticle;

        [Tooltip("The particle that plays when the Entity takes critical damage.")]
        public ParticleSystem criticalDamageParticle;

        [Header("Damage Audios")]
        [Tooltip("The Audio Clip that plays when the Entity takes damage.")]
        public AudioClip[] damageAudios;

        [Tooltip("The Audio Clip that plays when the Entity takes critical damage.")]
        public AudioClip[] criticalDamageAudios;

        [Header("Damage Flash")]
        [Tooltip("If true, the Entity flashes when taking damage.")]
        public bool flashOnDamage = true;

        [Tooltip("The color of the flash when the Entity takes damage.")]
        public Color damageColor = Color.red;

        [Header("Difficulty Change Text")]
        [Tooltip("A prefab that shows up when the Entity's difficulty is adjusted.")]
        public GameObject difficultyText;

        [Tooltip("The position offset for difficulty change text.")]
        public Vector3 difficultyTextOffset = new(0, 2, 0);

        [Tooltip("The skinned mesh renderers that flash when the Entity takes damage.")]
        public SkinnedMeshRenderer[] meshRenderers;

        [Header("Level Up")]
        [Tooltip("The Audio Clip that plays when the Entity levels up.")]
        public AudioClip levelUpAudio;

        [Tooltip("The particle instantiated when the Entity levels up.")]
        public ParticleSystem levelUpParticle;

        protected Entity m_entity;

        protected bool m_isFlashing;

        protected const float k_flashDuration = 0.25f;

        protected GameAudio m_audio => GameAudio.instance;

        protected virtual void InitializeEntity()
        {
            m_entity = GetComponent<Entity>();
            m_entity.onDamage.AddListener(OnEntityDamage);
            m_entity.stats.onLevelUp.AddListener(OnLevelUp);
        }

        protected virtual void OnEntityDamage(int amount, Vector3 _, bool critical)
        {
            InstantiateDamageText(amount, critical);
            PlayDamageParticle(critical);
            PlayDamageAudio(critical);
            FlashDamageColor();
        }

        protected virtual void OnLevelUp()
        {
            m_audio.PlayEffect(levelUpAudio);
            levelUpParticle?.Play();
        }

        protected virtual void InstantiateDamageText(int amount, bool critical)
        {
            var origin = transform.position + damageTextOffset;
            var instance = Instantiate(damageText, origin, Quaternion.identity);

            if (instance.TryGetComponent(out DamageText text))
            {
                text.target = transform;
                text.SetText(amount, critical);
            }
        }

        protected virtual void PlayDamageParticle(bool critical)
        {
            if (critical)
            {
                if (criticalDamageParticle)
                    criticalDamageParticle.Play();

                return;
            }

            if (damageParticle)
                damageParticle.Play();
        }

        protected virtual void PlayDamageAudio(bool critical)
        {
            if (critical)
            {
                PlayRandomAudio(criticalDamageAudios);
                return;
            }

            PlayRandomAudio(damageAudios);
        }

        protected virtual void PlayRandomAudio(AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0)
                return;

            var clip = clips[Random.Range(0, clips.Length)];
            m_audio.PlayEffect(clip);
        }

        protected virtual void FlashDamageColor()
        {
            if (!flashOnDamage || m_isFlashing)
                return;

            foreach (var meshRenderer in meshRenderers)
            {
                StartCoroutine(FlashColorRoutine(meshRenderer.material));
            }
        }

        protected virtual IEnumerator FlashColorRoutine(Material material)
        {
            var elapsedTime = 0f;
            var flashColor = damageColor;
            Color initialColor = Color.white; // Domyślny kolor, na wypadek braku `_Color` w materiale.

            // Sprawdź, czy materiał ma właściwość `_Color`
            if (material.HasProperty("_Color"))
            {
                initialColor = material.color;
            }
            else
            {
                Debug.LogWarning($"Material '{material.name}' does not have a '_Color' property.");
                yield break; // Przerwij coroutine, ponieważ materiał nie ma właściwości `_Color`.
            }

            m_isFlashing = true;

            while (elapsedTime < k_flashDuration)
            {
                elapsedTime += Time.deltaTime;
                var t = elapsedTime / k_flashDuration;
                material.color = Color.Lerp(flashColor, initialColor, t);
                yield return null;
            }

            material.color = initialColor;
            m_isFlashing = false;
        }

        /// <summary>
        /// Wyświetla komunikat o zmianie trudności nad przeciwnikiem.
        /// </summary>
        public void ShowDifficultyChange(bool increased)
        {
            if (m_entity == null || m_entity.stats == null)
            {
                Debug.LogError($"[AI-DDA] Entity or stats are NULL on {gameObject.name}! Skipping DifficultyText.");
                return;
            }

            if (m_entity.CompareTag("Entity/Player")) // 🔹 Ignorujemy zmiany dla gracza
            {
                Debug.Log($"[AI-DDA] Skipping DifficultyText for {gameObject.name} (Player).");
                return;
            }

            if (m_entity.isDead) // Sprawdzamy, czy przeciwnik jest martwy
            {
                Debug.Log($"[AI-DDA] Skipping DifficultyText for {gameObject.name} because it's dead.");
                return;
            }

            if (difficultyText == null)
            {
                Debug.LogError($"[AI-DDA] difficultyText is NULL on {gameObject.name}! Check the prefab assignment.");
                return;
            }

            Debug.Log($"[AI-DDA] ShowDifficultyChange() called for {gameObject.name}. Increased: {increased}");

            var origin = transform.position + difficultyTextOffset;
            var instance = Instantiate(difficultyText, origin, Quaternion.identity);

            if (instance.TryGetComponent(out DifficultyText text))
            {
                text.target = transform;

                string message = increased ? "Difficulty Increased" : "Difficulty Decreased";
                Color textColor = increased ? text.increaseColor : text.decreaseColor;

                text.SetText(message, textColor);
                Debug.Log($"[AI-DDA] DifficultyText instantiated at {origin}");
            }
            else
            {
                Debug.LogError("[AI-DDA] Instantiated object does not have DifficultyText component!");
            }

            Destroy(instance, 4f);
        }

        protected virtual void Start()
        {
            InitializeEntity();
        }
    }
}