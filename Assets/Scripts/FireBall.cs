using UnityEngine;

public class FireBall : MonoBehaviour
{
    public AudioSource FireCrackle;

    void OnAwake()
    {
        FireCrackle.Play();
    }
}
