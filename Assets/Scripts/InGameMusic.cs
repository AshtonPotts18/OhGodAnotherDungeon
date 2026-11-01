using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameMusic : MonoBehaviour
{

    public static InGameMusic Object { get; private set; }

    public AudioSource MusicSource;
    public AudioClip[] MusicForLevel;

    void Awake() // keeps the object from being destroyed
    {
        if (PlayerPrefs.HasKey("Music"))
        {
            float val = PlayerPrefs.GetFloat("Music");

            MusicSource.volume = val;
        }

        if (Object != null && Object != this)
        {
            Destroy(gameObject);
            return;
        }

        Object = this;

        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
        int index = SceneManager.GetActiveScene().buildIndex;
        if (index < MusicForLevel.Length)
        {
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
