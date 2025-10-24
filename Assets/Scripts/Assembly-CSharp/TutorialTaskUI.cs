using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialTaskUI : MonoBehaviour
{
    [Header("Refs (can be left unassigned; will auto-find)")]
    public RawImage overlay;
    public RawImage icon;
    public TextMeshProUGUI item;
    private HorizontalLayoutGroup layout;

    [Header("Assets")]
    public Texture checkedBox;

    [Header("Timing")]
    [SerializeField] private float fadeStart = 1.5f; // (unused before, keep if you plan to use later)
    [SerializeField] private float fadeTime = 1.5f;

    private float desiredPad;
    private float padUp;

    void Awake()
    {
        CacheRefs();

        if (layout)
        {
            desiredPad = layout.padding.left;
            layout.padding.left = 400;
            padUp = layout.padding.left;
        }
        else
        {
            // Fallback if layout missing
            desiredPad = 0f;
            padUp = 0f;
        }
    }

    // Try to recover missing references at runtime
    void CacheRefs()
    {
        if (!layout) layout = GetComponent<HorizontalLayoutGroup>();
        if (!overlay) overlay = GetComponentInChildren<RawImage>(true); // first RawImage as fallback
        if (!icon)
        {
            // Prefer a child named "Icon"
            foreach (var ri in GetComponentsInChildren<RawImage>(true))
                if (ri.name.ToLower().Contains("icon")) { icon = ri; break; }
            if (!icon) icon = overlay != null ? overlay : GetComponentInChildren<RawImage>(true);
        }
        if (!item) item = GetComponentInChildren<TextMeshProUGUI>(true);
    }

    public void StartFade()
    {
        CacheRefs();

        if (icon)
        {
            if (checkedBox) icon.texture = checkedBox;
            // CrossFadeAlpha is on Graphic; RawImage derives from it
            icon.CrossFadeAlpha(0f, fadeTime, ignoreTimeScale: true);
        }

        if (item) item.CrossFadeAlpha(0f, fadeTime, ignoreTimeScale: true);
        if (overlay) overlay.CrossFadeAlpha(0f, fadeTime, ignoreTimeScale: true);

        Invoke(nameof(DestroySelf), fadeTime);
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }

    public void SetItem(InventoryItem i, string text)
    {
        CacheRefs();

        // Replace control tags safely
        if (text == null) text = "";
        text = text.Replace("[inv]", $"[{InputManager.inventory}]");
        text = text.Replace("[m2]", $"[{InputManager.rightClick}]");

        if (item) item.text = text;

        // If you ever want to show an item icon, do it here, guarded:
        // if (icon) {
        //     var hasSprite = (i != null && i.iconTexture != null); // adjust field name if needed
        //     icon.texture  = hasSprite ? i.iconTexture : icon.texture;
        //     icon.enabled  = true; // or hasSprite;
        // }
    }

    void Update()
    {
        // Keep working even if layout is missing
        if (!layout) return;

        padUp = Mathf.Lerp(padUp, desiredPad, Time.deltaTime * 6f);

        var p = layout.padding; // struct copy
        p.left = (int)padUp;
        layout.padding = p;
    }

#if UNITY_EDITOR
    // Helpful in editor to prefill refs when you select the object
    void OnValidate()
    {
        if (!Application.isPlaying) CacheRefs();
    }
#endif
}
