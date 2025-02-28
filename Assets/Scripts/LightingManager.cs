
using UnityEngine;

[ExecuteAlways]
public class LightingManager : MonoBehaviour
{
    [SerializeField] private Light directionalLight;
    [SerializeField] private LightingPreset preset;
    [SerializeField] private ParticleSystem snow;
    [SerializeField, Range(0, 240)] private float timeOfDay;
    private int maxParticles;
    public float TimeMultiplier;
    private void Start()
    {
        timeOfDay = TimeMultiplier * .6f;

    }
    private void Update()
    {
        if (preset == null)
            return;
        if (Application.isPlaying)
        {
            timeOfDay += Time.deltaTime;
            timeOfDay %= TimeMultiplier;
            UpdateLighting(timeOfDay / TimeMultiplier);

        }
        else
        {
            UpdateLighting(timeOfDay / TimeMultiplier);

        }
    }
    private void UpdateLighting(float timePercent)
    {
        RenderSettings.ambientLight = preset.ambientColor.Evaluate(timePercent);
        RenderSettings.fogColor = preset.fogColor.Evaluate(timePercent);
        if (directionalLight != null)
        {
            directionalLight.color = preset.directionalColor.Evaluate(timePercent);
            directionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePercent * 360) - 90, 170, 0));
        }
        if (snow != null)
        {
            // Check if timePercent is between 75% and 25%
            if (timePercent >= 0.75f || timePercent < 0.25f)
            {
                maxParticles = 1000;
            }
            else
            {
                maxParticles = 0;
            }

            ParticleSystem.MainModule m = snow.main;
            m.maxParticles = maxParticles;
        }
    }

    private void OnValidate()
    {
        if (directionalLight != null)
            return;
        if (RenderSettings.sun != null)
        {
            directionalLight = RenderSettings.sun;
        }
        else
        {
            Light light = GameObject.FindFirstObjectByType<Light>();
        }

    }

}
