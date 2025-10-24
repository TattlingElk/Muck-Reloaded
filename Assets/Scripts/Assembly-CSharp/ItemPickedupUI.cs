using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

public class ItemPickedupUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI item;

    private HorizontalLayoutGroup layout;

    private float desiredPad;
    private float fadeStart = 6f;
    private float fadeTime = 1f;
    private float padLeft;

    void Awake()
    {
        CacheRefs();

        if (layout)
        {
            desiredPad = layout.padding.left;
            layout.padding = new RectOffset(-300, layout.padding.right, layout.padding.top, layout.padding.bottom);
            padLeft = layout.padding.left;
        }
        else
        {
            desiredPad = 0;
            padLeft = 0;
        }

        Invoke(nameof(StartFade), fadeStart);
    }

    void CacheRefs()
    {
        if (!layout) layout = GetComponent<HorizontalLayoutGroup>();
        if (!icon) icon = GetComponentInChildren<Image>(true);
        if (!item) item = GetComponentInChildren<TextMeshProUGUI>(true);
    }

    void StartFade()
    {
        if (icon) icon.CrossFadeAlpha(0f, fadeTime, ignoreTimeScale: true);
        if (item) item.CrossFadeAlpha(0f, fadeTime, ignoreTimeScale: true);
        Invoke(nameof(DestroySelf), fadeTime);
    }

    void DestroySelf()
    {
        Destroy(gameObject);
    }

    public void SetItem(InventoryItem i)
    {
        CacheRefs();

        if (i == null)
        {
            // Graceful fallback: no crash, still shows a toast
            if (item) item.text = "Item";
            if (icon) icon.enabled = false;
            return;
        }

        // Try to read amount (supports both field and property on decompiled types)
        int amount = 1;
        TryGetInt(i, "amount", ref amount);

        // Try to read sprite/icon (some decompilations use 'sprite', others 'icon')
        Sprite spr = null;
        TryGetSprite(i, "sprite", ref spr);
        if (!spr) TryGetSprite(i, "icon", ref spr);

        // Name should usually exist (ScriptableObject.name), but guard anyway
        string displayName = string.IsNullOrEmpty(i.name) ? "Item" : i.name;

        if (amount < 1)
        {
            if (icon) { icon.sprite = null; icon.enabled = false; }
            if (item) item.text = "Inventory full";
        }
        else
        {
            if (icon)
            {
                icon.sprite = spr;
                icon.enabled = (spr != null);
            }
            if (item) item.text = $"{amount}x {displayName}";
        }
    }

    public void SetPowerup(Powerup p)
    {
        CacheRefs();

        if (p == null)
        {
            if (icon) { icon.sprite = null; icon.enabled = false; }
            if (item) item.text = "Powerup";
            return;
        }

        // Powerup usually has 'sprite', but guard like above
        Sprite spr = null;
        TryGetSprite(p, "sprite", ref spr);
        if (icon)
        {
            icon.sprite = spr;
            icon.enabled = (spr != null);
        }

        string pname = string.IsNullOrEmpty(p.name) ? "Powerup" : p.name;
        string pdesc = "";
        TryGetString(p, "description", ref pdesc);

        if (item) item.text = string.IsNullOrEmpty(pdesc) ? pname : (pname + "\n<size=75%>" + pdesc);
    }

    void Update()
    {
        if (!layout) return;

        padLeft = Mathf.Lerp(padLeft, desiredPad, Time.deltaTime * 7f);
        var pad = new RectOffset(layout.padding.left, layout.padding.right, layout.padding.top, layout.padding.bottom);
        pad.left = (int)padLeft;
        layout.padding = pad;
    }

    // ---------- Reflection helpers for decompiled field/property name drift ----------

    static bool TryGetInt(object obj, string member, ref int value)
    {
        if (obj == null) return false;
        var t = obj.GetType();
        var f = t.GetField(member, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (f != null && f.FieldType == typeof(int)) { value = (int)f.GetValue(obj); return true; }
        var p = t.GetProperty(member, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (p != null && p.PropertyType == typeof(int)) { value = (int)p.GetValue(obj, null); return true; }
        return false;
    }

    static bool TryGetSprite(object obj, string member, ref Sprite value)
    {
        if (obj == null) return false;
        var t = obj.GetType();
        var f = t.GetField(member, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (f != null && typeof(Sprite).IsAssignableFrom(f.FieldType)) { value = (Sprite)f.GetValue(obj); return true; }
        var p = t.GetProperty(member, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (p != null && typeof(Sprite).IsAssignableFrom(p.PropertyType)) { value = (Sprite)p.GetValue(obj, null); return true; }
        return false;
    }

    static bool TryGetString(object obj, string member, ref string value)
    {
        if (obj == null) return false;
        var t = obj.GetType();
        var f = t.GetField(member, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (f != null && f.FieldType == typeof(string))
        {
            var v = (string)f.GetValue(obj);
            if (v != null) { value = v; return true; }
        }
        var p = t.GetProperty(member, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (p != null && p.PropertyType == typeof(string))
        {
            var v = (string)p.GetValue(obj, null);
            if (v != null) { value = v; return true; }
        }
        return false;
    }
}
