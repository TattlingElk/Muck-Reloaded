using System;
using TMPro;
using UnityEngine;

public class LobbySettings : MonoBehaviour
{
    public UiSettings difficultySetting;
    public UiSettings friendlyFireSetting;
    public UiSettings gamemodeSetting;

    // NEW: drag your duplicated "Shared Pickups" UiSettings here in the Inspector
    public UiSettings sharedPickupsSetting;

    public TMP_InputField seed;
    public GameObject startButton;

    public static LobbySettings Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        difficultySetting.AddSettings(1, Enum.GetNames(typeof(GameSettings.Difficulty)));
        friendlyFireSetting.AddSettings(0, Enum.GetNames(typeof(GameSettings.FriendlyFire)));
        gamemodeSetting.AddSettings(0, Enum.GetNames(typeof(GameSettings.GameMode)));

        // NEW: Off / On
        if (sharedPickupsSetting != null)
            sharedPickupsSetting.AddSettings(0, Enum.GetNames(typeof(GameSettings.SharedPickups)));
    }
}
