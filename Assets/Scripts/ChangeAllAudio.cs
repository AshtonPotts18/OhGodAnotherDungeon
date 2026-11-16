using UnityEngine;

public class ChangeAllAudio : MonoBehaviour
{
    [SerializeField] private string[] SavedVolumeName;

    [SerializeField] private string[] TagName; // name of the Tag

    private float clock = 0.05f;

    private void FixedUpdate()
    {
        if(clock < 0.0f)
       { 
            AllEffects();
            AllMusic();
            AllEnemies();
            clock = 0.05f;
        }
        else
        {
            clock = Time.deltaTime;
        }
    }

    private void AllEffects()
    {
        GameObject[] ObjectVolume = GameObject.FindGameObjectsWithTag(TagName[0]);

        foreach (GameObject Object in ObjectVolume)
        {
            

            AudioSource sound = Object.GetComponent<AudioSource>();

            if (sound != null)
            {

                if (PlayerPrefs.HasKey(SavedVolumeName[0]))
                {
                    float val = PlayerPrefs.GetFloat(SavedVolumeName[0]);
                    sound.volume = val;
                }

            }
        }
    }

    private void AllMusic()
    {
        GameObject[] ObjectVolume = GameObject.FindGameObjectsWithTag(TagName[1]);

        foreach (GameObject Object in ObjectVolume)
        {


            AudioSource sound = Object.GetComponent<AudioSource>();

            if (sound != null)
            {
                if (PlayerPrefs.HasKey(SavedVolumeName[1]))
                {
                    float val = PlayerPrefs.GetFloat(SavedVolumeName[1]);
                    sound.volume = val;
                }
               
            }
        }
    }

    private void AllEnemies()
    {
        GameObject[] ObjectVolume = GameObject.FindGameObjectsWithTag(TagName[2]);

        foreach (GameObject Object in ObjectVolume)
        {


            AudioSource sound = Object.GetComponent<AudioSource>();

            if (sound != null)
            {

                if (PlayerPrefs.HasKey(SavedVolumeName[2]))
                {
                    float val = PlayerPrefs.GetFloat(SavedVolumeName[2]);
                    sound.volume = val;
                }

            }
        }
    }

}
