using System.IO;
using UnityEngine;

public static class SaveSystem
{
    static string Dir => Path.Combine(Application.persistentDataPath, "Saves");
    static string SlotPath(int slot) => Path.Combine(Dir, $"slot{slot}.json");
    static string TmpPath(int slot) => Path.Combine(Dir, $"slot{slot}.tmp");

    public static void SaveToSlot(GameSave data, int slot)
    {
        if (!Directory.Exists(Dir)) Directory.CreateDirectory(Dir);
        var json = JsonUtility.ToJson(data, prettyPrint: false);

        var tmp = TmpPath(slot);
        var dst = SlotPath(slot);

        File.WriteAllText(tmp, json);
        if (File.Exists(dst))
        {
            // Atomic on Windows; on other platforms File.Replace may throw → fallback to overwrite
            try { File.Replace(tmp, dst, null); }
            catch { File.WriteAllText(dst, json); File.Delete(tmp); }
        }
        else File.Move(tmp, dst);
    }

    public static bool TryLoadSlot(int slot, out GameSave data)
    {
        var path = SlotPath(slot);
        if (!File.Exists(path)) { data = null; return false; }
        var json = File.ReadAllText(path);
        data = JsonUtility.FromJson<GameSave>(json);
        return data != null;
    }
}
