using System.Collections.Generic;

public static class GameData
{
    public enum Mode { Endless, Level }
    public static Mode currentMode = Mode.Endless;
    public static int currentLevel = 1;
}