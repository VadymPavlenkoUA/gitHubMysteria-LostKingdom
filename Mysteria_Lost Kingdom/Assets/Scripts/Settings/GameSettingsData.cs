using System;
using UnityEngine;

[Serializable]
public class GameSettingsData
{
    // DISPLAY
    public FullScreenMode fullScreenMode;
    public int resolutionIndex;
    public int qualityLevel;
    public int vSync;
    public int targetFPS;

    // URP / GRAPHICS
    public int msaaLevel;
    public float renderScale;
    public int shadowQuality;
    public int shadowResolution;
    public bool softShadows;
    public bool postProcessing;
    public bool ssao;

    // SLIDERS
    public int waterQuality;
    public int grassDensity;
    public int terrainQuality;
    public int viewDistance;
    public int detailLevel;

    public float fov;

    // Default preset
    public void SetDefaults()
    {
        fullScreenMode = FullScreenMode.FullScreenWindow;
        resolutionIndex = Screen.resolutions.Length - 1;
        qualityLevel = QualitySettings.GetQualityLevel();
        vSync = 1;
        targetFPS = 60;

        msaaLevel = 2;
        renderScale = 1f;
        shadowQuality = 2;
        shadowResolution = 2;
        softShadows = true;
        postProcessing = true;
        ssao = true;

        waterQuality = 5;
        grassDensity = 5;
        terrainQuality = 5;
        viewDistance = 5;
        detailLevel = 5;

        fov = 75f;
    }
}