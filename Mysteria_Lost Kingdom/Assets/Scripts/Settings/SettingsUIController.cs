using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SettingsUIManager : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown fullscreenDropdown;
    public TMP_Dropdown qualityDropdown;
    public Toggle vSyncToggle;
    public Slider fovSlider;

    private bool isLoading;

    private void Start()
    {
        InitResolutions();
        LoadToUI();
    }

    void InitResolutions()
    {
        resolutionDropdown.ClearOptions();

        var options = new System.Collections.Generic.List<string>();
        foreach (var res in Screen.resolutions)
        {
            //options.Add(res.width + "x" + res.height);
            int hz = Mathf.RoundToInt((float)res.refreshRateRatio.value);
            options.Add($"{res.width}x{res.height} @{hz}Hz");
        }

        resolutionDropdown.AddOptions(options);
    }

    void LoadToUI()
    {
        isLoading = true;

        var settings = SettingsManager.Instance.CurrentSettings;

        resolutionDropdown.value = settings.resolutionIndex;
        fullscreenDropdown.value = (int)settings.fullScreenMode;
        qualityDropdown.value = settings.qualityLevel;
        vSyncToggle.isOn = settings.vSync == 1;
        fovSlider.value = settings.fov;

        isLoading = false;
    }

    public void OnResolutionChanged(int index)
    {
        if (isLoading) return;
        SettingsManager.Instance.CurrentSettings.resolutionIndex = index;
    }

    public void OnFullscreenChanged(int mode)
    {
        if (isLoading) return;
        SettingsManager.Instance.CurrentSettings.fullScreenMode = (FullScreenMode)mode;
    }

    public void OnQualityChanged(int level)
    {
        if (isLoading) return;
        SettingsManager.Instance.CurrentSettings.qualityLevel = level;
    }

    public void OnVSyncChanged(bool state)
    {
        if (isLoading) return;
        SettingsManager.Instance.CurrentSettings.vSync = state ? 1 : 0;
    }

    public void OnFOVChanged(float value)
    {
        if (isLoading) return;
        SettingsManager.Instance.CurrentSettings.fov = value;
    }

    public void Apply()
    {
        SettingsManager.Instance.ApplyAllSettings();
        SettingsManager.Instance.SaveSettings();
    }

    public void Reset()
    {
        SettingsManager.Instance.ResetSettings();
        LoadToUI();
    }
}