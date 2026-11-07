using UnityEngine;

public class Torch : UsableItem
{
    [Header("Torch Settings")]
    [SerializeField, Min(0.1f)] private float burnDuration = 10f;
    [SerializeField, Min(0f)] private float lightIntensity = 1f;
    [SerializeField, Range(0f, 1f)] private float minimumFlameSize = 0.25f;

    [Header("Model Prefabs")]
    [SerializeField] private GameObject litModel;
    [SerializeField] private GameObject unlitModel;

    [Header("Lit Torch Effects")]
    [SerializeField] private Light torchLight;
    [SerializeField] private ParticleSystem flameParticles;

    private const string HoldPointName = "HoldPoint";

    private MeshFilter torchMeshFilter;
    private MeshRenderer torchMeshRenderer;

    private bool isExtinguished;
    private float remainingBurnTime;
    private float startingEmissionMultiplier = 1f;
    private float startingParticleSizeMultiplier = 1f;
    private float nextBurnLogTime;

    private void Awake()
    {
        burnDuration = Mathf.Max(0.1f, burnDuration);
        remainingBurnTime = burnDuration;

        torchMeshFilter = FindComponent<MeshFilter>(gameObject);
        torchMeshRenderer = FindComponent<MeshRenderer>(gameObject);

        ApplyModel(litModel);

        if (flameParticles != null)
        {
            startingEmissionMultiplier =
                flameParticles.emission.rateOverTimeMultiplier;

            startingParticleSizeMultiplier =
                flameParticles.main.startSizeMultiplier;
        }

        UpdateBurnVisuals();
    }

    public override void SetHeld(bool held)
    {
        base.SetHeld(held);
        Debug.Log($"[Torch] SetHeld({held}) on {name}", this);
    }

    public override void UseItem()
    {
        // The torch starts burning while it is under HoldPoint.
    }

    private void Update()
    {
        if (!IsUnderHoldPoint() || isExtinguished)
            return;

        remainingBurnTime = Mathf.Max(
            0f,
            remainingBurnTime - Time.deltaTime
        );

        UpdateBurnVisuals();

        if (Time.time >= nextBurnLogTime)
        {
            Debug.Log(
                $"[Torch] Burning: {remainingBurnTime:F1}s left.",
                this
            );

            nextBurnLogTime = Time.time + 1f;
        }

        if (remainingBurnTime <= 0f)
            Extinguish();
    }

    private bool IsUnderHoldPoint()
    {
        Transform currentParent = transform.parent;

        while (currentParent != null)
        {
            if (currentParent.name == HoldPointName)
                return true;

            currentParent = currentParent.parent;
        }

        return false;
    }

    private void UpdateBurnVisuals()
    {
        float remainingRatio = Mathf.Clamp01(
            remainingBurnTime / burnDuration
        );

        if (torchLight != null)
            torchLight.intensity = lightIntensity * remainingRatio;

        if (flameParticles != null)
        {
            var emission = flameParticles.emission;
            emission.rateOverTimeMultiplier =
                startingEmissionMultiplier * remainingRatio;

            var main = flameParticles.main;
            main.startSizeMultiplier =
                startingParticleSizeMultiplier *
                Mathf.Lerp(minimumFlameSize, 1f, remainingRatio);
        }
    }

    private void Extinguish()
    {
        if (isExtinguished)
            return;

        isExtinguished = true;
        remainingBurnTime = 0f;

        if (torchLight != null)
        {
            torchLight.intensity = 0f;
            torchLight.enabled = false;
        }

        if (flameParticles != null)
        {
            flameParticles.Stop(
                true,
                ParticleSystemStopBehavior.StopEmitting
            );
        }

        if (!ApplyModel(unlitModel))
            Debug.LogError(
                "[Torch] Could not find a Mesh Filter on the Unlit Model prefab.",
                this
            );

        Debug.Log("[Torch] Extinguished; changed to the unlit model.", this);
    }

    private bool ApplyModel(GameObject modelPrefab)
    {
        if (modelPrefab == null || torchMeshFilter == null)
            return false;

        MeshFilter sourceMeshFilter =
            FindComponent<MeshFilter>(modelPrefab);

        if (sourceMeshFilter == null)
            return false;

        torchMeshFilter.sharedMesh = sourceMeshFilter.sharedMesh;

        MeshRenderer sourceRenderer =
            FindComponent<MeshRenderer>(modelPrefab);

        if (sourceRenderer != null && torchMeshRenderer != null)
        {
            torchMeshRenderer.sharedMaterials =
                sourceRenderer.sharedMaterials;

            torchMeshRenderer.enabled = sourceRenderer.enabled;
        }

        return true;
    }

    private static T FindComponent<T>(GameObject target)
        where T : Component
    {
        if (target == null)
            return null;

        T component = target.GetComponent<T>();

        if (component != null)
            return component;

        return target.GetComponentInChildren<T>(true);
    }
}