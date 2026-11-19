using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class SoundSettings : MonoBehaviour
{
    [SerializeField] private string[] TagName; // name of the Tag

    [SerializeField] private string[] SavedVolumeName;
    public Slider EnemySlider; // This is for the Slider so the player can change the volume;

    public Slider MusicSlider;
    public Slider SFXSlider;

    public Slider MasterSlider;


    void Start()
    {
        if(PlayerPrefs.HasKey("MasterVolume"))
        {
            AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume");
        }

        if (PlayerPrefs.HasKey(TagName[0]))
        {
            GameObject[] ObjectVolume = GameObject.FindGameObjectsWithTag(TagName[0]);
            foreach (GameObject Object in ObjectVolume)
            {
                AudioSource sound = Object.GetComponent<AudioSource>();

                if (sound != null)
                {
                    sound.volume = PlayerPrefs.GetFloat(SavedVolumeName[0]);
                }
            }
        }

        if (PlayerPrefs.HasKey(TagName[1]))
        {
            GameObject[] ObjectVolume = GameObject.FindGameObjectsWithTag(TagName[1]);
            foreach (GameObject Object in ObjectVolume)
            {
                AudioSource sound = Object.GetComponent<AudioSource>();

                if (sound != null)
                {
                    sound.volume = PlayerPrefs.GetFloat(SavedVolumeName[1]);
                }
            }
        }

        if (PlayerPrefs.HasKey(TagName[2]))
        {
            GameObject[] ObjectVolume = GameObject.FindGameObjectsWithTag(TagName[2]);
            foreach (GameObject Object in ObjectVolume)
            {
                AudioSource sound = Object.GetComponent<AudioSource>();

                if (sound != null)
                {
                    sound.volume = PlayerPrefs.GetFloat(SavedVolumeName[2]);
                }
            }
        }

    }


    public void SetVolumeForTag() //Depending on the Tag of the GameObject it will change all sounds for all the tags with a sound.
    {

        GameObject[] ObjectVolume = GameObject.FindGameObjectsWithTag(TagName[0]);

        foreach (GameObject Object in ObjectVolume)
        {

            AudioSource sound = Object.GetComponent<AudioSource>();

            if (sound != null)
            {
                PlayerPrefs.SetFloat(SavedVolumeName[0], EnemySlider.value);
                sound.volume = EnemySlider.value;
            }
        }
    }

    public void SetVolumeForSFX() //Depending on the Tag of the GameObject it will change all sounds for all the tags with a sound.
    {

        GameObject[] ObjectVolume = GameObject.FindGameObjectsWithTag(TagName[1]);

        foreach (GameObject Object in ObjectVolume)
        {

            AudioSource sound = Object.GetComponent<AudioSource>();

            if (sound != null)
            {
                PlayerPrefs.SetFloat(SavedVolumeName[1], SFXSlider.value);
                sound.volume = SFXSlider.value;
            }
        }
    }

    public void SetVolumeForMusic() //Depending on the Tag of the GameObject it will change all sounds for all the tags with a sound.
    {

        GameObject[] ObjectVolume = GameObject.FindGameObjectsWithTag(TagName[2]);

        foreach (GameObject Object in ObjectVolume)
        {

            AudioSource sound = Object.GetComponent<AudioSource>();

            if (sound != null)
            {
                PlayerPrefs.SetFloat(SavedVolumeName[2], MusicSlider.value);
                sound.volume = MusicSlider.value;
            }
        }
    }

    public void MasterVolume()
    {
        PlayerPrefs.SetFloat("MasterVolume", MasterSlider.value);
        AudioListener.volume = MasterSlider.value;
    }


    public void OnTagVolValueChanged(float v)
    {
        SetVolumeForTag();
    }

    public void OnSFXVolValueChanged(float v)
    {
        SetVolumeForSFX();
    }

    public void OnMusicVolValueChanged(float v)
    {
        SetVolumeForMusic();
    }

    public void OnMasterVolValueChanged(float v)
    {
        MasterVolume();
    }
}
