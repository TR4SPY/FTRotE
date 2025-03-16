using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    public class DeadEntityState : EntityState
    {
        public override void Enter(Entity entity)
        {
            var anim = entity.GetComponentInChildren<Animator>();
            if (anim)
            {
                anim.ResetTrigger("OnAttack");
                anim.ResetTrigger("OnStunned");
                anim.ResetTrigger("OnBlock");
                anim.SetFloat("Speed", 0f);
                anim.SetTrigger("OnDie");
                anim.Play("Dying", 0, 0f);
            }
        }

        public override void Exit(Entity entity)
        {
           // Debug.Log($"[DeadEntityState] Exit on {entity.name}");
        }

        public override void Step(Entity entity)
        {
            // Wywołuje się co klatkę
            // Debug.Log($"[DeadEntityState] Step on {entity.name}");
            // Nic nie robimy – postać ma pozostać „martwa”
        }
    }
}
