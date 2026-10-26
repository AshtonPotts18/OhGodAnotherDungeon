using UnityEngine;
using UnityEngine.UI;

public class SavedVolume : MonoBehaviour
{
    [SerializeField] private string SavedVolumeName;


    [SerializeField] private Slider Bar;

    void Start()
    {
        if (PlayerPrefs.HasKey(SavedVolumeName))
        {
            float val = PlayerPrefs.GetFloat(SavedVolumeName);

            Bar.value = val;
        }
        else
        {

        }

    }

    public void OnSliderValueChanged(float v)
    {
        PlayerPrefs.SetFloat(SavedVolumeName, v);
    }

}
