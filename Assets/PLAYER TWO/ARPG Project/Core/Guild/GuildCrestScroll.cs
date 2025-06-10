using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [CreateAssetMenu(fileName = "New Guild Crest Scroll", menuName = "PLAYER TWO/ARPG Project/Item/Guild Crest Scroll")]
    public class GuildCrestScroll : Item
    {
        [Tooltip("List of crest sprites available in this scroll.")]
        public Sprite[] crests;
    }
}
