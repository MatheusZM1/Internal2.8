using System.Collections.Generic;
using UnityEngine;

public class ControlOptions
{
    [Header("Controller Options")]
    public static int controllerRumble;
    public static float controllerDeadZone;
    public static int deadzoneIndex;
    public static List<float> deadzoneList = new List<float> { 0f, 0.05f, 0.1f, 0.15f, 0.2f, 0.25f, 0.3f, 0.35f, 0.4f, 0.45f, 0.5f, 0.55f, 0.6f, 0.65f, 0.7f, 0.75f, 0.8f, 0.85f, 0.9f };

    public static void Initialize()
    {
        LoadControls();
    }

    public static void LoadControls()
    {
        controllerRumble = PlayerPrefs.GetInt("controllerRumble", 0);
        deadzoneIndex = PlayerPrefs.GetInt("deadzoneIndex", 4);
        controllerDeadZone = deadzoneList[deadzoneIndex];
    }

    public static void ResetControls()
    {
        PlayerPrefs.DeleteKey("controllerRumble");
        PlayerPrefs.DeleteKey("deadzoneIndex");
        LoadControls();
    }

    public static void ChangeDeadzone(int changeSign)
    {
        deadzoneIndex += 1 * changeSign;
        if (deadzoneIndex > deadzoneList.Count - 1) deadzoneIndex = deadzoneList.Count - 1;
        else if (deadzoneIndex < 0) deadzoneIndex = 0;
        controllerDeadZone = deadzoneList[deadzoneIndex];
        PlayerPrefs.SetInt("deadzoneIndex", deadzoneIndex);
        PlayerPrefs.Save();
    }
}
