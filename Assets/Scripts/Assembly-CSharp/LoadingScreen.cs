using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    public TextMeshProUGUI text;
    public RawImage loadingBar;
    public RawImage background;

    private float desiredLoad;
    private Graphic[] allGraphics;

    public CanvasGroup canvasGroup;

    public Transform loadingParent;
    public GameObject loadingPlayerPrefab;

    public static LoadingScreen Instance;

    public bool[] players;                // ready flags by player id
    public CanvasGroup loadBar;
    public CanvasGroup playerStatuses;
    public GameObject[] loadingObject;

    public bool loadingInGame;

    private float currentFadeTime;
    private float desiredAlpha;

    public float totalFadeTime { get; set; } = 1f;

    private void Awake()
    {
        Instance = this;

        if (canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        if (background) background.gameObject.SetActive(false);

        // allow up to 10 players by default; adjust if you have lobby size
        players = new bool[10];

        if (LocalClient.serverOwner)
        {
            InvokeRepeating(nameof(CheckAllPlayersLoading), 5f, 5f);
        }
    }

    private void CheckAllPlayersLoading()
    {
        if (GameManager.state == GameManager.GameState.Playing)
        {
            CancelInvoke(nameof(CheckAllPlayersLoading));
            return;
        }
        Debug.LogError("Checking all players");
        foreach (Client value in Server.clients.Values)
        {
            if (value?.player != null && !value.player.loading)
            {
                ServerSend.StartGame(value.player.id, GameManager.gameSettings);
                Debug.LogError(value.player.username + " failed to load, trying to get him to load again...");
            }
        }
    }

    private void Start()
    {
        if (loadingInGame)
        {
            InitLoadingPlayers();
        }
    }

    public void SetText(string s, float loadProgress)
    {
        if (background) background.gameObject.SetActive(true);
        if (text) text.text = s;
        desiredLoad = loadProgress;
    }

    // --------- HARDENED HIDERS / SHOWERS ----------

    public void Hide(float fadeTime = 1f)
    {
        desiredAlpha = 0f;
        totalFadeTime = fadeTime;
        currentFadeTime = 0f;

        if (canvasGroup)
        {
            if (fadeTime == 0f) canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        if (background) background.gameObject.SetActive(false);

        CancelInvoke(nameof(HideStuff));
        Invoke(nameof(HideStuff), totalFadeTime <= 0f ? 0f : totalFadeTime);
    }

    private void HideStuff()
    {
        if (canvasGroup) canvasGroup.alpha = 0f;
        if (background) background.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    public void FinishLoading()
    {
        if (loadingObject != null)
            foreach (var go in loadingObject) if (go) go.SetActive(false);

        if (playerStatuses) playerStatuses.alpha = 1f; // show the block if you have one

        if (loadingParent) loadingParent.gameObject.SetActive(true); // <-- show rows now
        if (loadBar) loadBar.alpha = 0f;

        // don't hide the whole overlay here if you want the “players ready” page visible;
        // if you prefer hiding entirely, call Hide(0f) instead and skip showing loadingParent.
    }


    public void Show(float fadeTime = 1f)
    {
        desiredAlpha = 1f;
        currentFadeTime = 0f;
        totalFadeTime = fadeTime;

        gameObject.SetActive(true);

        if (canvasGroup)
        {
            if (fadeTime == 0f) canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
        if (background) background.gameObject.SetActive(true);
    }

    // --------- READY / ADVANCE LOGIC ----------

    public void UpdateStatuses(int id)
    {
        // mark player as ready
        if (id >= 0)
        {
            if (players == null || players.Length <= id)
            {
                // expand if needed
                var newLen = Mathf.Max(id + 1, players != null ? players.Length : 0);
                System.Array.Resize(ref players, Mathf.Max(newLen, 1));
            }
            players[id] = true;
        }

        // update UI row text if present
        if (loadingParent && loadingParent.childCount > id && id >= 0)
        {
            var pl = loadingParent.GetChild(id).GetComponent<PlayerLoading>();
            if (pl) pl.ChangeStatus("<color=green>Ready");
        }

        // host/solo fallback: if everyone shows ready, advance immediately
        if (LocalClient.serverOwner && AllPlayersReadyOrSoloHost())
        {
            FinishLoading();
            if (GameManager.instance) GameManager.instance.StartGame();
        }
    }

    public bool AllPlayersReadyOrSoloHost()
    {
        int expected = NetworkController.Instance ? NetworkController.Instance.nPlayers : 1;
        if (expected <= 1) return true;

        if (players == null || players.Length == 0) return false;

        int ready = 0;
        for (int i = 0; i < expected && i < players.Length; i++)
            if (players[i]) ready++;

        return ready >= expected;
    }

    // --------- ROW CREATION ----------

    public void InitLoadingPlayers()
    {
        if (loadingParent) loadingParent.gameObject.SetActive(false); // keep hidden while loading
        for (int i = 0; i < NetworkController.Instance.playerNames.Length; i++)
        {
            var pl = Object.Instantiate(loadingPlayerPrefab, loadingParent).GetComponent<PlayerLoading>();
            pl.SetStatus(NetworkController.Instance.playerNames[i], "<color=red>Loading");
        }
    }


    private void Update()
    {
        if (loadingBar)
            loadingBar.transform.localScale = new Vector3(desiredLoad, 1f, 1f);

        if (currentFadeTime < totalFadeTime && totalFadeTime > 0f && canvasGroup)
        {
            currentFadeTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, desiredAlpha, currentFadeTime / totalFadeTime);
        }
    }
}
