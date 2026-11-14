using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class AudioApply : MonoBehaviour
{
    public AudioSource Source;
    public List<AudioClip> Attack_Sounds;
    public List<AudioClip> Damaged_Sounds;
    public List<AudioClip> Death_Sounds;

    public void OnAttacking()
    {
        int randomIndex = Random.Range(0, Attack_Sounds.Count - 1);
        Source.PlayOneShot(Attack_Sounds[randomIndex]);
    }

    public void OnDamaged()
    {
        int randomIndex = Random.Range(0, Damaged_Sounds.Count - 1);
        Source.PlayOneShot(Damaged_Sounds[randomIndex]);
    }

    public void OnDeath()
    {
        int randomIndex = Random.Range(0, Death_Sounds.Count - 1);
        AudioSource.PlayClipAtPoint(Death_Sounds[randomIndex], transform.position); 
    }

    void OnDestroy()
    {
        OnDeath();
    }
}
