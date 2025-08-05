using System;

namespace PLAYERTWO.ARPGProject
{
    [Serializable]
    public class BuffInstance
    {
        public Buff buff;
        public float remainingTime;
        public float remainingCooldown;
        public bool isActive;

        public bool isDebuff;

        public BuffInstance(Buff buff, bool isDebuff = false)
        {
            this.buff = buff;
            this.isDebuff = isDebuff;

            remainingTime = buff.removeOnLogout ? float.PositiveInfinity : buff.duration;
            remainingCooldown = 0f;
            isActive = true;
        }
    }
}
