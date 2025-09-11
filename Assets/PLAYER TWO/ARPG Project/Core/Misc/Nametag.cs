using UnityEngine;
using UnityEngine.UI;

namespace PLAYERTWO.ARPGProject
{
    [RequireComponent(typeof(Billboard))]
    public class Nametag : MonoBehaviour
    {
        [Header("References")]
        public Text characterText;
        public Text guildText;
        public Text classText;
        public Image guildCrestImage;

        public Transform target;

        [Header("Offsets")]
        public Vector3 worldOffset = new Vector3(0, 2f, 0);

        private void Start()
        {
            transform.localScale = Vector3.one;
        }

        private void LateUpdate()
        {
            if (target != null)
            {
                transform.position = target.position + worldOffset;

                var cam = Camera.main;
                if (cam != null)
                {
                    /*
                    float distance = Vector3.Distance(transform.position, cam.transform.position);
                    float scaleFactor = distance * 0.05f;
                    transform.localScale = Vector3.one * scaleFactor;
                    */
                }
            }
        }

        public void SetNametag(string playerName, int level, string guild = "", string className = "")
        {
            bool showInfo = GameSettings.instance == null || GameSettings.instance.GetDisplayPlayerInfo();

            if (showInfo)
                characterText.text = StringUtils.StringWithColorAndStyle($"{playerName} (Lv.{level})", GameColors.White, bold: true);
            else
                characterText.text = StringUtils.StringWithColorAndStyle(playerName, GameColors.White, bold: true);

            bool showGuild = !string.IsNullOrEmpty(guild) && (GameSettings.instance == null || GameSettings.instance.GetDisplayGuildName());
            if (showGuild)
            {
                guildText.gameObject.SetActive(true);
                guildText.text = StringUtils.StringWithColorAndStyle($"< {guild} >", GameColors.White, bold: true);

                var crest = GuildManager.GetCurrentGuildCrest();
                if (crest != null && guildCrestImage != null)
                {
                    guildCrestImage.sprite = crest;
                    guildCrestImage.gameObject.SetActive(true);

                    var layoutRoot = guildCrestImage.transform.parent as RectTransform;
                    if (layoutRoot != null)
                        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);
                }
                else if (guildCrestImage != null)
                {
                    guildCrestImage.gameObject.SetActive(false);
                }
            }
            else
            {
                guildText.gameObject.SetActive(false);
                if (guildCrestImage != null)
                    guildCrestImage.gameObject.SetActive(false);

            }

            if (!string.IsNullOrEmpty(className) && showInfo)
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
