using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameMusic : MonoBehaviour
{

    public AudioSource MusicSource;
    public AudioClip[] MusicForLevel;

    void Awake() // keeps the object from being destroyed
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        int index = SceneManager.GetActiveScene().buildIndex;
        if (index < MusicForLevel.Length)
        {
            MusicSource.Stop();
            MusicSource.clip = MusicForLevel[index];
            MusicSource.loop = true;
            MusicSource.Play();
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

}
