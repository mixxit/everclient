using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

// Nice utility class for passing
// Scene parameters
// https://forum.unity3d.com/threads/unity-beginner-loadlevel-with-arguments.180925/

// Example
// Scenes.Load("Level2", "showAds", "true");
// in Level2 scene:
// bool showAds = Scenes.getParam("showAds") == "true";

public static class Scenes
{
    private static Dictionary<string, string> parameters;

    public static void Load(string sceneName, Dictionary<string, string> parameters = null)
    {
        Scenes.parameters = parameters;
        SceneManager.LoadScene(sceneName);
    }

    public static void Load(string sceneName, string paramKey, string paramValue)
    {
        Scenes.parameters = new Dictionary<string, string>();
        Scenes.parameters.Add(paramKey, paramValue);
        SceneManager.LoadScene(sceneName);
    }

    public static Dictionary<string, string> getSceneParameters()
    {
        return parameters;
    }

    public static string getParam(string paramKey)
    {
        if (parameters == null) return "";
        return parameters[paramKey];
    }

    public static void setParam(string paramKey, string paramValue)
    {
        if (parameters == null)
            Scenes.parameters = new Dictionary<string, string>();
        Scenes.parameters.Add(paramKey, paramValue);
    }

}