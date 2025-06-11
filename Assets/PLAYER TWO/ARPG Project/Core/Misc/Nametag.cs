using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PLAYERTWO.ARPGProject
{
    [RequireComponent(typeof(Billboard))]
    public class Nametag : MonoBehaviour
    {
        [Header("References")]
        public Text characterText;
        public TMP_Text guildText;
        public Text classText;

        public Transform target;

        [Header("Offsets")]
        public Vector3 worldOffset = new Vector3(0, 2f, 0);

        private void LateUpdate()
        {
            if (target != null)
            {
                transform.position = target.position + worldOffset;

                var cam = Camera.main;
                if (cam != null)
                {
                    float distance = Vector3.Distance(transform.position, cam.transform.position);
                    float scaleFactor = distance * 0.05f;
                    transform.localScale = Vector3.one * scaleFactor;
                }
            }
        }

        public void SetNametag(string playerName, int level, string guild = "", string className = "")
        {
            characterText.text = StringUtils.StringWithColorAndStyle($"{playerName} (Lv.{level})", GameColors.White, bold: true);

            if (!string.IsNullOrEmpty(guild))
            {
                guildText.gameObject.SetActive(true);
                string guildFormatted = $"< {guild} >";

                var crest = GuildManager.instance?.currentGuildCrest;
                if (crest != null)
                {
                    string spriteName = crest.name;
                    guildFormatted = $"<sprite name=\"{spriteName}\" tint> {guildFormatted}";
                }

                guildText.text = StringUtils.StringWithColorAndStyle(guildFormatted, GameColors.White, bold: true);
            }
            else
            {
                guildText.gameObject.SetActive(false);
            }

            if (!string.IsNullOrEmpty(className))
            {
                classText.gameObject.SetActive(true);
                classText.text = StringUtils.StringWithColor(className, GameColors.White);
            }
            else
            {
                classText.gameObject.SetActive(false);
            }
        }
    }
}
