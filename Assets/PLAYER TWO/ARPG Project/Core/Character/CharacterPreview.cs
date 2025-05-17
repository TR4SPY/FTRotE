using System.Collections;
using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [AddComponentMenu("PLAYER TWO/ARPG Project/Character/Character Preview")]
    public class CharacterPreview : Singleton<CharacterPreview>
    {
        [Header("Character Preview Settings")]
        [Tooltip("The slot where the character preview will be displayed.")]
        public Transform characterSlot;

        protected Coroutine m_instantiateRoutine;
        protected Entity m_entity;
        private CharacterInstance m_currentInstance;
        private int m_currentPreviewToken = 0;

        /// <summary>
        /// Preview the character in the character slot.
        /// </summary>
        /// <param name="character">The character to preview.</param>
        public virtual void Preview(CharacterInstance character)
        {
            Clear();

            m_currentInstance = character;
            m_currentPreviewToken++;
            int token = m_currentPreviewToken;

            m_instantiateRoutine = StartCoroutine(InstantiateEntity(character, token));
        }

        /// <summary>
        /// Clear the character preview.
        /// </summary>
        // CharacterPreview.cs
        // CharacterPreview.cs
        public virtual void Clear()
        {
                    StopInstantiation();

                    if (m_entity == null) return; 

            /*  Dlaczego DestroyImmediate:
            *  • podglądowe GameObjecty są w pełni odizolowane (AI, Input, Collision – OFF),
            *    więc ich natychmiastowe usunięcie nie dotyka reszty gry;
            *  • muszą zniknąć PRZED stworzeniem następnego modelu,
            *    bo odroczony Destroy() wywoła OnDestroy starego prefab-u
            *    po rejestracji nowego i „wyczyści” go z managerów.
            */
            Object.DestroyImmediate(m_entity.gameObject);
            m_entity = null; 
        }


        protected virtual void StopInstantiation()
        {
            if (m_instantiateRoutine == null)
                return;

            StopCoroutine(m_instantiateRoutine);
            m_instantiateRoutine = null;
        }

        protected virtual IEnumerator InstantiateEntity(CharacterInstance character, int token)
        {
            m_entity = character.Instantiate();

            if (token != m_currentPreviewToken ||
                m_currentInstance == null ||
                character.data != m_currentInstance.data ||
                m_entity == null)
                yield break;

            m_entity.enabled = false;
            if (m_entity.inputs) m_entity.inputs.enabled = false;
            var ctrl = m_entity.GetComponent<CharacterController>();
            if (ctrl) ctrl.enabled = false;

            m_entity.transform.SetPositionAndRotation(characterSlot.position,
                                                    characterSlot.rotation);
            m_entity.gameObject.SetActive(true);

        }
    }
}
