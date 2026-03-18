using System.IO;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    public UniversalRenderPipelineAsset urpAsset;
    public Camera mainCamera;

    public GameSettingsData CurrentSettings;

    private string settingsPath;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        settingsPath = Path.Combine(Application.persistentDataPath, "settings.json");

        LoadSettings();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene loaded: " + scene.name);

        mainCamera = Camera.main;
        ApplyAllSettings();
    }

    #region LOAD / SAVE

    public void LoadSettings()
    {
        if (!File.Exists(settingsPath))
        {
            CurrentSettings = new GameSettingsData();
            CurrentSettings.SetDefaults();
            SaveSettings();
        }
        else
        {
            string json = File.ReadAllText(settingsPath);
            CurrentSettings = JsonUtility.FromJson<GameSettingsData>(json);
        }

        ApplyAllSettings();
    }

    public void SaveSettings()
    {
        string json = JsonUtility.ToJson(CurrentSettings, true);
        File.WriteAllText(settingsPath, json);

        Debug.Log("SAVED JSON:\n" + json);
    }

    public void ResetSettings()
    {
        CurrentSettings.SetDefaults();
        ApplyAllSettings();
        SaveSettings();
    }

    #endregion

    #region APPLY

    public void ApplyAllSettings()
    {
        Debug.Log("Applying settings...");
        ApplyResolution();
        ApplyQuality();
        //ApplyURP();
        ApplyCamera();
    }

    void ApplyResolution()
    {
        Debug.Log("Resolution index: " + CurrentSettings.resolutionIndex);
        Resolution res = Screen.resolutions[CurrentSettings.resolutionIndex];
        Screen.SetResolution(res.width, res.height, CurrentSettings.fullScreenMode);
    }

    //void ApplyQuality()
    //{
    //    QualitySettings.SetQualityLevel(CurrentSettings.qualityLevel);
    //    QualitySettings.vSyncCount = CurrentSettings.vSync;
    //    Application.targetFrameRate = CurrentSettings.targetFPS;
    //}

    void ApplyQuality()
    {
        QualitySettings.SetQualityLevel(CurrentSettings.qualityLevel);
        QualitySettings.vSyncCount = CurrentSettings.vSync;
        Application.targetFrameRate = CurrentSettings.targetFPS;

        var asset = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
        if (asset == null) return;

        switch (CurrentSettings.qualityLevel)
        {
            case 0: // Ultra
                asset.renderScale = 1f;
                asset.msaaSampleCount = 8;
                QualitySettings.shadowDistance = 120f;
                QualitySettings.shadows = UnityEngine.ShadowQuality.All;
                break;

            case 1: // High
                asset.renderScale = 1f;
                asset.msaaSampleCount = 4;
                QualitySettings.shadowDistance = 75f;
                QualitySettings.shadows = UnityEngine.ShadowQuality.All;
                break;

            case 2: // Medium
                asset.renderScale = 0.85f;
                asset.msaaSampleCount = 2;
                QualitySettings.shadowDistance = 50f;
                QualitySettings.shadows = UnityEngine.ShadowQuality.All;
                break;

            case 3: // Low
                asset.renderScale = 0.7f;
                asset.msaaSampleCount = 1;
                QualitySettings.shadowDistance = 25f;
                QualitySettings.shadows = UnityEngine.ShadowQuality.All;
                break;

        }
    }

    //void ApplyURP()
    //{
    //    if (urpAsset == null) return;

    //    urpAsset.msaaSampleCount = CurrentSettings.msaaLevel;
    //    urpAsset.renderScale = CurrentSettings.renderScale;

    //    QualitySettings.shadowResolution =
    //(UnityEngine.ShadowResolution)CurrentSettings.shadowResolution;

    //    QualitySettings.shadows =
    //        CurrentSettings.shadowQuality > 0
    //        ? UnityEngine.ShadowQuality.All
    //        : UnityEngine.ShadowQuality.Disable;

    //    QualitySettings.shadowDistance =
    //CurrentSettings.shadowQuality > 0 ? 100f : 0f;
    //}

    void ApplyCamera()
    {
        //if (mainCamera != null) mainCamera.fieldOfView = CurrentSettings.fov;
    }

    #endregion
}