using UnityEngine;
using System.Collections;
using AI_DDA.Assets.Scripts;

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
        private bool isDisplayingDifficultyChange;
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
            float mult = GameSettings.instance ? GameSettings.instance.GetFlashReductionMultiplier() : 1f;

            if (critical)
            {
                if (criticalDamageParticle)
                {
                    var main = criticalDamageParticle.main;
                    if (main.startColor.mode == ParticleSystemGradientMode.Color)
                    {
                        Color c = main.startColor.color * mult;
                        main.startColor = new ParticleSystem.MinMaxGradient(c);
                    }
                    criticalDamageParticle.Play();
                }

                return;
            }

            if (damageParticle)
            {
                var main = damageParticle.main;
                if (main.startColor.mode == ParticleSystemGradientMode.Color)
                {
                    Color c = main.startColor.color * mult;
                    main.startColor = new ParticleSystem.MinMaxGradient(c);
                }
                damageParticle.Play();
            }
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
            var flashColor = damageColor * (GameSettings.instance ? GameSettings.instance.GetFlashReductionMultiplier() : 1f);
            Color initialColor = Color.white;

            if (material.HasProperty("_Color"))
            {
                initialColor = material.color;
            }
            else
            {
                // Debug.LogWarning($"Material '{material.name}' does not have a '_Color' property.");
                yield break; 
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
        public void ShowDifficultyChange(DifficultyText.MessageType messageType)
        {
            if (isDisplayingDifficultyChange) return;

            isDisplayingDifficultyChange = true;
            StartCoroutine(ResetDisplayFlag());

            if (m_entity == null || m_entity.stats == null) 
                return;

            if (m_entity.isDead) 
                return;

            if (DifficultyManager.Instance == null)
            {
                Debug.LogError("[AI-DDA] DifficultyManager instance is NULL!");
                return;
            }

            int difficultyOption = GameSettings.instance.GetDifficultyTextOption();
            bool showOverEnemies = (difficultyOption == 0);
            bool showOverPlayer  = (difficultyOption == 1);
            if (showOverEnemies && !m_entity.CompareTag("Entity/Enemy"))
                return;
            if (showOverPlayer && !m_entity.CompareTag("Entity/Player"))
                return;

            if (showOverPlayer && DifficultyText.activeTexts.Exists(t => t && t.target == transform))
                return;

            if (difficultyText == null)
            {
                Debug.LogError($"[AI-DDA] difficultyText is NULL on {gameObject.name}! Check the prefab assignment.");
                return;
            }

            Vector3 origin = transform.position + new Vector3(0, 2f, 0);
            var instance = Instantiate(difficultyText, origin, Quaternion.identity);
            if (instance.TryGetComponent(out DifficultyText text))
            {
                text.target = transform;
                text.SetMessageType(messageType);

                if (difficultyOption == 1)
                {
                    string[] messages = null;
                    switch (messageType)
                    {
                        case DifficultyText.MessageType.DifficultyDecreased:
                        case DifficultyText.MessageType.BuffUp:
                            messages = text.positivePlayerMessages;
                            break;
                        case DifficultyText.MessageType.DifficultyIncreased:
                        case DifficultyText.MessageType.WeakenUp:
                            messages = text.negativePlayerMessages;
                            break;
                    }

                    if (messages != null && messages.Length > 0)
                    {
                        text.difficultyText.text = messages[Random.Range(0, messages.Length)];
                    }
                }
                Debug.Log($"[AI-DDA] DifficultyText instantiated at {origin} for {gameObject.name}");
            }
            else
            {
                Debug.LogError("[AI-DDA] Instantiated object does not have DifficultyText component!");
            }

            Destroy(instance, 4f);
        }

        private IEnumerator ResetDisplayFlag()
        {
            yield return new WaitForSeconds(0.5f);
            isDisplayingDifficultyChange = false;
        }

        protected virtual void Start()
        {
            InitializeEntity();
        }
    }
}