using UnityEngine;

public class DestroySmoke : MonoBehaviour
{
    [System.Obsolete]
    void Start()
    {
        ParticleSystem ps = gameObject.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            float totalDuration = ps.duration;
            Destroy(gameObject, totalDuration);
        }
    }

}
