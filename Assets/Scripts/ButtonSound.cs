using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections;

public class ButtonSound : MonoBehaviour
{
    public AudioSource Audio;

    public string scene;

    public float delay = 0.5f;

    IEnumerator WaitAndDoSomething()
    {
        if (Audio != null)
        {
            Audio.Play(); // Plays the assigned clip
            yield return new WaitForSecondsRealtime(delay);
            SceneManager.LoadScene(scene);

        }

    }

    public void PlayOneShot()
    {
        if (Audio != null)
        {
            Audio.Play(); // Plays the assigned clip
        }
    }

    public void PlayThenSwitch()
    {
        StartCoroutine(WaitAndDoSomething());
    }

}
