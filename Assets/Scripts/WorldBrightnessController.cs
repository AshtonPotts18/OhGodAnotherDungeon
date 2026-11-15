using UnityEngine;
using UnityEngine.UI;
public class WorldBrightnessController : MonoBehaviour
{
    [SerializeField] private Slider brightnessSlider;
    [SerializeField, Range(0f, 1f)] private float maxAmbientBrightness = 0.18f;
    [SerializeField] private Color ambientTint = Color.white;
    private float savedValue = 0.5f;
    private const string SaveKey = "World Brightness";

    // Load the saved brightness and apply it when this scene starts.
    void Start()
    {
        if(PlayerPrefs.HasKey(SaveKey))
        {
            savedValue = PlayerPrefs.GetFloat(SaveKey, brightnessSlider != null ? brightnessSlider.value : 0f);
            if (brightnessSlider != null)
            {
                brightnessSlider.SetValueWithoutNotify(savedValue);
                brightnessSlider.onValueChanged.AddListener(SetBrightness);
            }
        }
        SetBrightness(savedValue);
        SetBrightness(0.5f);
    }

    // Applies the slider value to the world's ambient brightness and saves the preference.
    public void SetBrightness(float sliderValue)
    {
        float brightness = Mathf.Clamp01(sliderValue) * maxAmbientBrightness;
        RenderSettings.ambientLight = new Color(ambientTint.r * brightness, ambientTint.g * brightness, ambientTint.b * brightness, 1f);
        PlayerPrefs.SetFloat(SaveKey, sliderValue);
    }

    private void OnDestroy()
    {
        if (brightnessSlider != null)
        {
            brightnessSlider.onValueChanged.RemoveListener(SetBrightness);
        }
    }
}
