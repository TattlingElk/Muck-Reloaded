using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSaveManager : MonoBehaviour
{
    public static GameSaveManager Instance { get; private set; }

    [Header("Config")]
    [Tooltip("Which save slot to use (1..N)")]
    public int currentSlot = 1;

    [Header("References")]
    [Tooltip("Player root transform (drag your player here)")]
    public Transform player;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5)) SaveNow();  // quick-save
        if (Input.GetKeyDown(KeyCode.F9)) LoadNow();  // quick-load
    }

    public void SaveNow()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null) { Debug.LogWarning("SaveNow: No player found."); return; }

        var gs = new GameSave
        {
            saveVersion = 1,
            gameVersion = Application.version,
            sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
            day = 0,
            utcTicks = System.DateTime.UtcNow.Ticks,
            player = new PlayerData(),
            world = new WorldData { seed = 0, objects = new SavedObjectState[0] }
        };

        var p = player;
        gs.player.pos = new float[] { p.position.x, p.position.y, p.position.z };
        gs.player.rot = new float[] { p.eulerAngles.x, p.eulerAngles.y, p.eulerAngles.z };
        gs.player.hp = 0; gs.player.stamina = 0; gs.player.money = 0;
        gs.player.inventoryItemIds = new int[0];
        gs.player.powerupIds = new int[0];

        SaveSystem.SaveToSlot(gs, currentSlot);
        Debug.Log($"[Save] slot {currentSlot} written.");
    }


    public void LoadNow()
    {
        if (!SaveSystem.TryLoadSlot(currentSlot, out var gs))
        {
            Debug.LogWarning($"[Load] Slot {currentSlot} not found.");
            return;
        }
        StartCoroutine(LoadRoutine(gs));
    }

    System.Collections.IEnumerator LoadRoutine(GameSave gs)
    {
        // Swap scene if needed
        if (SceneManager.GetActiveScene().name != gs.sceneName)
        {
            var op = SceneManager.LoadSceneAsync(gs.sceneName);
            while (!op.isDone) yield return null;
        }

        // Re-find player if scene changed or reference lost
        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player != null)
        {
            player.position = new Vector3(gs.player.pos[0], gs.player.pos[1], gs.player.pos[2]);
            player.eulerAngles = new Vector3(gs.player.rot[0], gs.player.rot[1], gs.player.rot[2]);
        }
        else Debug.LogWarning("LoadRoutine: Player transform not set/found.");

        // TODO later: restore hp/stamina/money/inventory/powerups

        // TODO later: regenerate world from seed, then apply Saveable deltas

        Debug.Log($"[Load] Loaded slot {currentSlot}.");
    }
}
