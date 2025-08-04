using UnityEngine;
using System.Collections.Generic;

namespace PLAYERTWO.ARPGProject
{
    [System.Serializable]
    public class BuffsSerializer
    {
        [System.Serializable]
        public class BuffData
        {
            public int buffId = -1;
            public bool isDebuff;
            public float remainingTime;
            public float remainingCooldown;
            public bool isActive;
        }

        public List<BuffData> buffs = new();

        public BuffsSerializer() {}

        public BuffsSerializer(EntityBuffManager manager)
        {
            if (manager == null) return;
            foreach (var instance in manager.buffs)
            {
                buffs.Add(new BuffData
                {
                    buffId = GameDatabase.instance.GetElementId<Buff>(instance.buff),
                    isDebuff = instance.isDebuff,
                    remainingTime = instance.remainingTime,
                    remainingCooldown = instance.remainingCooldown,
                    isActive = instance.isActive
                });
            }
        }

        public void ApplyTo(EntityBuffManager manager)
        {
            if (manager == null) return;
            foreach (var data in buffs)
            {
                var buff = GameDatabase.instance.FindElementById<Buff>(data.buffId);
                if (buff == null) continue;
                manager.RestoreBuff(buff, data.isDebuff, data.remainingTime, data.remainingCooldown, data.isActive);
            }
        }

        public virtual string ToJson() => JsonUtility.ToJson(this);
        public static BuffsSerializer FromJson(string json) =>
            JsonUtility.FromJson<BuffsSerializer>(json);
    }
}
