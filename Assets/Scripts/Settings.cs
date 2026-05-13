using UnityEngine;

public static class Settings
{
    public static bool Debug;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        var settings = Resources.Load<GameSettings>("GameSettings");
        
        Debug = settings.Debug;
    }
}
