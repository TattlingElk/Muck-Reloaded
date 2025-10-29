public class GameSettings
{
    public enum GameMode
    {
        Survival,
        Versus,
        Creative
    }

    public enum FriendlyFire
    {
        Off,
        On
    }

    public enum Difficulty
    {
        Easy,
        Normal,
        Gamer
    }

    public enum Respawn
    {
        OnNewDay,
        Never
    }

    public enum GameLength
    {
        Short = 3,
        Medium = 8,
        Long = 14
    }

    public enum Multiplayer
    {
        Off,
        On
    }

    // NEW: shared pickups toggle
    public enum SharedPickups
    {
        Off,
        On
    }

    public int Seed;

    public GameMode gameMode { get; set; }

    public FriendlyFire friendlyFire { get; set; }

    public Difficulty difficulty { get; set; }

    public Respawn respawn { get; set; }

    public GameLength gameLength { get; set; }

    public Multiplayer multiplayer { get; set; }

    // NEW: expose shared pickups
    public SharedPickups sharedPickups { get; set; }

    // Added trailing optional param for sharedPickups; defaults to Off
    public GameSettings(int seed, GameMode gameMode = GameMode.Survival, FriendlyFire friendlyFire = FriendlyFire.Off, Difficulty difficulty = Difficulty.Normal, GameLength gameLength = GameLength.Short, Multiplayer multiplayer = Multiplayer.On, SharedPickups sharedPickups = SharedPickups.Off)
    {
        Seed = seed;
        this.gameMode = gameMode;
        this.friendlyFire = friendlyFire;
        this.difficulty = difficulty;
        this.gameLength = gameLength;
        this.multiplayer = multiplayer;
        this.sharedPickups = sharedPickups; // NEW
    }

    // Added final int param for sharedPickups
    public GameSettings(int seed, int gameMode, int friendlyFire, int difficulty, int gameLength, int multiplayer, int sharedPickups)
    {
        Seed = seed;
        this.gameMode = (GameMode)gameMode;
        this.friendlyFire = (FriendlyFire)friendlyFire;
        this.difficulty = (Difficulty)difficulty;
        this.gameLength = (GameLength)gameLength;
        this.multiplayer = (Multiplayer)multiplayer;
        this.sharedPickups = (SharedPickups)sharedPickups; // NEW
    }

    public int BossDay()
    {
        switch (difficulty)
        {
            case Difficulty.Easy:
                return 6;
            case Difficulty.Normal:
                return 4;
            case Difficulty.Gamer:
                return 3;
            default:
                return 5;
        }
    }

    public float GetChestPriceMultiplier()
    {
        switch (difficulty)
        {
            case Difficulty.Easy:
                return 8f;
            case Difficulty.Normal:
                return 6f;
            case Difficulty.Gamer:
                return 5f;
            default:
                return 5f;
        }
    }

    public int DayLength()
    {
        switch (difficulty)
        {
            case Difficulty.Easy:
                return 56;
            case Difficulty.Normal:
                return 54;
            case Difficulty.Gamer:
                return 52;
            default:
                return 5;
        }
    }
}
