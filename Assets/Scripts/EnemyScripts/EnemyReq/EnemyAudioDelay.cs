using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemyAudioDelay : MonoBehaviour
{
    [Header("Sounds")]
    [SerializeField] private AudioClip[] soundClips;

    [Header("Timing")]
    [SerializeField, Min(0.1f)] private float minimumInterval = 4f;
    [SerializeField, Min(0.1f)] private float maximumInterval = 8f;

    private AudioSource audioSource;
    private float nextSoundTime;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        ScheduleNextSound();
    }

    private void Update()
    {
        if (soundClips == null || soundClips.Length == 0) { return; }

        if (Time.time < nextSoundTime || audioSource.isPlaying) { return; }

        AudioClip clip = soundClips[Random.Range(0, soundClips.Length)];

        if (clip != null) { audioSource.PlayOneShot(clip); }

        ScheduleNextSound();
    }

    private void ScheduleNextSound()
    {
        float interval = Random.Range(minimumInterval,
            Mathf.Max(minimumInterval, maximumInterval));

        nextSoundTime = Time.time + interval;
    }
}
