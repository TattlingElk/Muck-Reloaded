[System.Serializable]
public class GameSave
{
    public int saveVersion = 1;
    public string gameVersion;
    public string sceneName;
    public int day;
    public long utcTicks;
    public PlayerData player;
    public WorldData world;
}

[System.Serializable]
public class PlayerData
{
    public float[] pos;     // x,y,z
    public float[] rot;     // euler x,y,z
    public int hp;
    public int stamina;
    public int money;
    public int[] powerupIds;
    public int[] inventoryItemIds;
}

[System.Serializable]
public class WorldData
{
    public int seed;
    public SavedObjectState[] objects;  // deltas for opened/picked/destroyed
}

[System.Serializable]
public class SavedObjectState
{
    public string guid;
    public bool destroyed;
    public string extraJson;
}
