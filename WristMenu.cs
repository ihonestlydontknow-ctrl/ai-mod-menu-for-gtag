using System;
using UnityEngine;

public static class WristMenu
{
    // This allows your AllMods script to register its buttons without crashing
    public static void Register(string buttonName, Action<bool> callback)
    {
        Debug.Log($"[Mod Menu] Registered button: {buttonName}");
    }
}
