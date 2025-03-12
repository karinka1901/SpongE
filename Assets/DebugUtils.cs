using UnityEngine;

public static class DebugUtils
///////////////////DEBUGGING WITH COLORS/////////////////////
{
    public static void LogColor(string message, string color)
    {
        Debug.Log($"<color={color}>{message}</color>");

        //DebugUtils.LogColor("text", "color");
    }

    public static void Log(string message)
    {
        LogColor(message, "white");
    }
}
