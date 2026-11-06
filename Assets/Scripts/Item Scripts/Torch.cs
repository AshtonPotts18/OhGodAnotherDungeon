using UnityEngine;

public class Torch : MonoBehaviour
{
    [Header("Torch Settings")]
    [SerializeField] private float burnDuration = 10f;
    [SerializeField] private float lightIntensity = 1f;
    [SerializeField] private float minimumFlameSize = 0.25f;

    [Header("Torch Assets")]
    [SerializeField] private GameObject torch_lit;
    [SerializeField] private GameObject torch_extinguished;
    [SerializeField] private Light torchLight;
    [SerializeField] private ParticleSystem flameParticles;

    [Header("Current State")]
    [SerializeField] private bool isEquipped;

    private float currentBurnTime;
    private float startingEmissionRate;
    private float startingParticleSize;
    private bool isExtinguished;

    void Start()
    {
        currentBurnTime = burnDuration;

        if(flameParticles != null)
        {
            ParticleSystem.EmissionModule emission = flameParticles.emission;
            ParticleSystem.MainModule main = flameParticles.main;

            startingEmissionRate = flameParticles.emission.rateOverTime.constant;
            startingParticleSize = flameParticles.main.startSize.constant;
        }

        if(torch_lit != null && torch_extinguished != null)
        {
            torch_lit.SetActive(true);
            torch_extinguished.SetActive(false);
        }

        UpdateVisuals();
    }
    void UpdateVisuals()
    {
        //dims the light slowly to the players default light source as the torch burns out
        if (torchLight == null) return;
        
        float remainingRatio = burnDuration > 0 ? currentBurnTime / burnDuration : 0f;
        torchLight.intensity = lightIntensity * remainingRatio;

        float burnRate = burnDuration > 0 ? Mathf.Clamp01(currentBurnTime / burnDuration) : 0f;
        
        if (torchLight != null)
        {
            torchLight.intensity = lightIntensity * burnRate;
        }

        if (flameParticles != null)
        {
            ParticleSystem.EmissionModule emission = flameParticles.emission;
            ParticleSystem.MainModule main = flameParticles.main;
            
            emission.rateOverTimeMultiplier = startingEmissionRate * burnRate;
            main.startSizeMultiplier = startingParticleSize * Mathf.Lerp(minimumFlameSize, 1f, burnRate);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsTorchEquipped() || isExtinguished)
        {
            return;
        }

        BurnTorch();
    }

    bool IsTorchEquipped()
    {
        return isEquipped;
    }

    void BurnTorch()
    {
        currentBurnTime -= Time.deltaTime;
        currentBurnTime = Mathf.Max(currentBurnTime, 0f);

        UpdateVisuals();

        if (currentBurnTime <= 0f)
        {
            Extinguish();
        }
    }

    void Extinguish()
    {
        if(isExtinguished) return;

        isExtinguished = true;
        currentBurnTime = 0f;

        if(torchLight != null)
        {
            torchLight.enabled = false;
        }

        if (flameParticles != null)
        {
            flameParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (torch_lit != null) { torch_lit.SetActive(false); }

        if (torch_extinguished != null) { torch_extinguished.SetActive(true); }

        //Extinguish sound goes here.
    }
}
