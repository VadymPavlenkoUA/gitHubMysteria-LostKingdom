using UnityEngine;

public class TimeOfDayLighting : MonoBehaviour
{
    public Light sunLight;
    public Light moonLight;

    [Header("Presets")]
    public Gradient sunColor;
    public AnimationCurve sunIntensity;

    [Header("Ambient")]
    public Gradient ambientColor;
    public AnimationCurve ambientIntensity;

    [Header("Skybox")]
    public Material skyboxMaterial;

    public Gradient skyTint;
    public AnimationCurve skyExposure;

    [Header("Fog")]
    public Gradient fogColor;
    public AnimationCurve fogDensity;

    private void Update()
    {
        float time = TimeOfDayManager.Instance.timeOfDay / 24f;

        float sunAngle = time * 360f - 90f;
        sunLight.transform.rotation = Quaternion.Euler(sunAngle, 270f, 0f);

        sunLight.color = sunColor.Evaluate(time);
        sunLight.intensity = sunIntensity.Evaluate(time);

        //moonLight.intensity = TimeOfDayManager.Instance.IsNight ? 0.5f : 0f;

        float daylight = sunIntensity.Evaluate(time);
        RenderSettings.ambientLight = ambientColor.Evaluate(time);
        RenderSettings.ambientIntensity = ambientIntensity.Evaluate(time);

        skyboxMaterial.SetFloat(
            "_Exposure",
            skyExposure.Evaluate(time)
        );

        skyboxMaterial.SetColor(
            "_SkyTint",
            skyTint.Evaluate(time)
        );

        DynamicGI.UpdateEnvironment();

        RenderSettings.fogColor = fogColor.Evaluate(time);
        RenderSettings.fogDensity = fogDensity.Evaluate(time);
    }
}
