using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiSettings : MonoBehaviour
{
    [Header("Prefab with Button root + TMP child")]
    public GameObject settingButton;

    [Header("Optional colors")]
    [SerializeField] Color selected = Color.white;
    [SerializeField] Color deselected = Color.gray;

    private TextMeshProUGUI[] texts;
    public int setting { get; private set; }

    public void AddSettings(int defaultValue, string[] enumNames)
    {
        // Basic validation
        if (settingButton == null)
        {
            Debug.LogError($"{name}/UiSettings: settingButton prefab not assigned.");
            return;
        }
        if (enumNames == null || enumNames.Length == 0)
        {
            Debug.LogWarning($"{name}/UiSettings: enumNames was null/empty; using placeholder.");
            enumNames = new[] { "(missing)" };
        }

        // Clear previous children (in case this gets called multiple times)
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        setting = Mathf.Clamp(defaultValue, 0, enumNames.Length - 1);
        texts = new TextMeshProUGUI[enumNames.Length];

        for (int i = 0; i < enumNames.Length; i++)
        {
            int index = i;
            var go = Instantiate(settingButton, transform);
            var button = go.GetComponent<Button>();
            if (button == null)
            {
                Debug.LogError($"{name}/UiSettings: settingButton prefab has no Button component.");
                continue;
            }
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => UpdateSetting(index));

            // Grab TMP from children
            var tmp = go.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp == null)
            {
                Debug.LogError($"{name}/UiSettings: settingButton prefab is missing a TextMeshProUGUI child.");
                continue;
            }
            tmp.text = enumNames[i];
            texts[i] = tmp;
        }

        UpdateSelection();
    }

    private void UpdateSetting(int i)
    {
        setting = Mathf.Clamp(i, 0, texts != null ? texts.Length - 1 : 0);
        UpdateSelection();
        // TODO: if you need to propagate the chosen value to config, do it here.
    }

    private void UpdateSelection()
    {
        if (texts == null) return;
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] == null) continue;
            texts[i].color = (i == setting) ? selected : deselected;
        }
    }
}
