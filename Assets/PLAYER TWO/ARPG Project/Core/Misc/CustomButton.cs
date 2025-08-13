using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class SaturationTintButton : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler
{
    public Image targetImage;

    [Range(0f, 1f)] public float normalSaturation = 0.2f;
    [Range(0f, 1f)] public float highlightedSaturation = 1.0f;
    [Range(0f, 1f)] public float disabledSaturation = 0.05f;

    public Color normalTint = Color.white;
    public Color pressedTint = new Color(0.85f, 0.85f, 0.85f);
    public Color disabledTint = new Color(0.6f, 0.6f, 0.6f);

    public float transitionSpeed = 8f;
    public float pressedScale = 0.95f;

    private Material runtimeMat;
    private Button button;
    private float targetSaturation;
    private Color targetTint;
    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool externallyHidden = false;

    void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponentInChildren<Image>();

        runtimeMat = Instantiate(targetImage.material);
        targetImage.material = runtimeMat;

        runtimeMat.SetFloat("_Saturation", normalSaturation);
        runtimeMat.SetColor("_TintColor", normalTint);

        button = GetComponent<Button>();
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    void OnEnable()
    {
        externallyHidden = (transform.localScale == Vector3.zero);

        if (!externallyHidden)
            UpdateStateImmediate();
    }

    void Update()
    {
        if (transform.localScale == Vector3.zero)
        {
            externallyHidden = true;
            return;
        }
        else if (externallyHidden)
        {
            externallyHidden = false;
            UpdateStateImmediate();
        }

        float currentSat = runtimeMat.GetFloat("_Saturation");
        runtimeMat.SetFloat("_Saturation", Mathf.Lerp(currentSat, targetSaturation, Time.deltaTime * transitionSpeed));

        Color currentTint = runtimeMat.GetColor("_TintColor");
        runtimeMat.SetColor("_TintColor", Color.Lerp(currentTint, targetTint, Time.deltaTime * transitionSpeed));

        if (transform.localScale != targetScale)
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * transitionSpeed);

        if (!button.interactable && targetSaturation != disabledSaturation)
        {
            SetState(disabledSaturation, disabledTint, originalScale);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable) return;
        SetState(highlightedSaturation, normalTint, originalScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!button.interactable) return;
        SetState(normalSaturation, normalTint, originalScale);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable) return;
        SetState(highlightedSaturation, pressedTint, originalScale * pressedScale);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!button.interactable) return;
        SetState(highlightedSaturation, normalTint, originalScale);
    }

    private void SetState(float saturation, Color tint, Vector3 scale)
    {
        targetSaturation = Mathf.Clamp01(saturation);
        targetTint = tint;
        targetScale = scale;
    }

    private void UpdateStateImmediate()
    {
        if (!button.interactable)
            SetState(disabledSaturation, disabledTint, originalScale);
        else
            SetState(normalSaturation, normalTint, originalScale);

        runtimeMat.SetFloat("_Saturation", targetSaturation);
        runtimeMat.SetColor("_TintColor", targetTint);
        transform.localScale = targetScale;
    }

    public void Retarget(Image newTarget)
    {
        if (newTarget == null || newTarget == targetImage) return;

        if (targetImage && runtimeMat && ReferenceEquals(targetImage.material, runtimeMat))
            targetImage.material = null;

        targetImage = newTarget;

        if (runtimeMat) Destroy(runtimeMat);
        runtimeMat = Instantiate(targetImage.material);
        targetImage.material = runtimeMat;

        UpdateStateImmediate();
    }
}
