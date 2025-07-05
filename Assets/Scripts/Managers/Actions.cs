using System;

public static class Actions
{
    // Game
    public static Action levelReset;

    // Game menus
    public static Action<bool> onGamePause;
    public static Action onGameDeath;
    public static Action onLoadout;
}