using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameMusic : MonoBehaviour
{

    public AudioSource MusicSource;
    public AudioClip[] MusicForLevel;

    private static InGameMusic music;

    void Awake() // keeps the object from being destroyed
    {
        if(music != null && music != this)
        {
            Destroy(gameObject);
            return;
        }

        music = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        int index = SceneManager.GetActiveScene().buildIndex;
        if (index < MusicForLevel.Length)
        {
            if (PlayerPrefs.HasKey("Music"))
            {
                MusicSource.volume = PlayerPrefs.GetFloat("Music");
            }


            if(MusicSource.isPlaying)
            { MusicSource.Stop(); }
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
