using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class SmokeEffect : MonoBehaviour
{
    public GameObject smokePuffPrefab;
    public AudioClip Sizzle;

    [System.Obsolete]
    void OnDestroy()
    {
        if (smokePuffPrefab != null)
        {
            GameObject smokeInstance = Instantiate(smokePuffPrefab, transform.position, Quaternion.identity);
            AudioSource.PlayClipAtPoint(Sizzle, transform.position, 1.0f);

            ParticleSystem ps = smokeInstance.GetComponent<ParticleSystem>();
            float totalDuration = ps.duration + ps.main.startLifetime.constantMax;

            Destroy(smokeInstance, totalDuration);
        }
    }
}
