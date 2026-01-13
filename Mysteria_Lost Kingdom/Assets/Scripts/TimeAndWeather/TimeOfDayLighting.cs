using UnityEngine;

public class TimeOfDayLighting : MonoBehaviour
{
    [Header("Sun")]
    public Light sunLight;
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

    [Header("Moon")]
    public Light moonLight;
    public Gradient moonColor;
    public AnimationCurve moonIntensity;

    private void Update()
    {
        float time = TimeOfDayManager.Instance.timeOfDay / 24f;

        float sunAngle = time * 360f - 90f;
        sunLight.transform.rotation = Quaternion.Euler(sunAngle, 270f, 0f);
        sunLight.color = sunColor.Evaluate(time);
        sunLight.intensity = sunIntensity.Evaluate(time);

        float moonAngle = sunAngle + 180f;
        moonLight.transform.rotation = Quaternion.Euler(moonAngle, 270f, 0f);
        moonLight.color = moonColor.Evaluate(time);
        moonLight.intensity = moonIntensity.Evaluate(time);

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