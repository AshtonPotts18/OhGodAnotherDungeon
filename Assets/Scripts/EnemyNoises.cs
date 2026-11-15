using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class EnemyNoises : MonoBehaviour
{
    public AudioSource Source;
    public List<AudioClip> ClipsAttack;
    public List<AudioClip> ClipsDamaged;
    public List<AudioClip> ClipsDeath;
    private EnemyHealth EnemyDamaged;
    private float Comparer;
    void Start()
    {
        EnemyDamaged = GetComponent<EnemyHealth>();
        Comparer = EnemyDamaged.CurrentHealth;
    }

    void Update()
    {
        if(EnemyDamaged.CurrentHealth < Comparer)
        {
            if(!Source.isPlaying)
            { Source.PlayOneShot(ClipsDamaged[Random.Range(0, ClipsDamaged.Count)]); }
            Comparer = EnemyDamaged.CurrentHealth;
        }
    }

    private void OnDestroy()
    {
        Source.Stop();
        AudioSource.PlayClipAtPoint(ClipsDeath[Random.Range(0, ClipsDeath.Count)], transform.position, 1.0f);
    }


    public void OnDamage()
    {
         Source.PlayOneShot(ClipsAttack[Random.Range(0, ClipsAttack.Count)]); 
    }

}
