using UnityEngine;
using UnityEngine.UI;
public class SoundSettings : MonoBehaviour
{
    [SerializeField] private string TagName; // name of the Tag

    [SerializeField] private string SavedVolumeName;
    public Slider SettingsSlider; // This is for the Slider so the player can change the volume;


    public Slider MasterSlider;


    
 
    public void SetVolumeForTag() //Depending on the Tag of the GameObject it will change all sounds for all the tags with a sound.
    {

        GameObject[] ObjectVolume = GameObject.FindGameObjectsWithTag(TagName);

        foreach (GameObject Object in ObjectVolume)
        {

            AudioSource sound = Object.GetComponent<AudioSource>();

            if (sound != null)
            {
                PlayerPrefs.SetFloat(SavedVolumeName, SettingsSlider.value);
                sound.volume = SettingsSlider.value;
            }
        }
    }

    public void MasterVolume()
    {
        PlayerPrefs.SetFloat("MasterVolume", SettingsSlider.value);
        AudioListener.volume = MasterSlider.value;
    }


    public void OnTagVolValueChanged(float v)
    {
        SetVolumeForTag();
    }

    public void OnMasterVolValueChanged(float v)
    {
        MasterVolume();
    }
}
