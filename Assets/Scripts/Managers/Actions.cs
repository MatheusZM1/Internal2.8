using System;

public static class Actions
{
    // Players
    public static Action<bool> onPlayerTwoConnect;

    // Game
    public static Action startLevel;
    public static Action resetProjectiles;
    public static Action levelReset;
    public static Action levelEnd;

    // Game menus
    public static Action<bool> onGamePause;
    public static Action onGameDeath;
    public static Action onLoadout;
}