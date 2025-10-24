using UnityEngine;

public class Saveable : MonoBehaviour
{
    [SerializeField] string guid;   // Assign once in inspector (unique!)
    public string Guid => guid;

    // Example payload toggles; replace with your own fields
    [SerializeField] bool destroyed;

    public SavedObjectState ToState()
    {
        return new SavedObjectState
        {
            guid = guid,
            destroyed = destroyed,
            extraJson = "" // TODO: serialize any custom fields if needed
        };
    }

    public void FromState(SavedObjectState s)
    {
        destroyed = s.destroyed;
        if (destroyed) gameObject.SetActive(false);
        // TODO: parse s.extraJson and apply custom fields
    }
}

#if UNITY_EDITOR
// tiny editor helper to generate a GUID from the context menu
[UnityEditor.CustomEditor(typeof(Saveable))]
public class SaveableEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var s = (Saveable)target;
        if (string.IsNullOrEmpty(s.name)) return;
        if (GUILayout.Button("Generate GUID (once)"))
        {
            var so = new UnityEditor.SerializedObject(s);
            so.FindProperty("guid").stringValue = System.Guid.NewGuid().ToString("N");
            so.ApplyModifiedProperties();
        }
    }
}
#endif
