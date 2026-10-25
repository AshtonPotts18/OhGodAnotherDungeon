using UnityEngine;
using UnityEngine.Audio;

public class ButtonSound : MonoBehaviour
{
    public AudioSource Audio;

    public void PlayOneShot()
    {
        if (Audio != null)
        {
            Audio.Play(); // Plays the assigned clip
        }
    }

}
