using UnityEngine;
using UnityEngine.UI;
public class SoundSettings : MonoBehaviour
{
    [SerializeField] private string TagName; // name of the Tag
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
                sound.volume = SettingsSlider.value;
            }
        }
    }

    public void MasterVolume()
    {
        AudioListener.volume = MasterSlider.value;
    }
}
