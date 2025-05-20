using UnityEngine;

namespace PLAYERTWO.ARPGProject
{
    [RequireComponent(typeof(GUIInventory))]
    public class InventoryPreviewController : MonoBehaviour
    {
        GUIInventory inv;

        void Awake()  => inv = GetComponent<GUIInventory>();
        void OnEnable()  => SetPreviews(true);
        void OnDisable() => SetPreviews(false);

        void SetPreviews(bool state)
        {
            foreach (var it in inv.itemsContainer
                                  .GetComponentsInChildren<GUIItem>(true))
                it.SetPreviewActive(state);
        }
    }
}
