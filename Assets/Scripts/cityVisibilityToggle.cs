using UnityEngine;

public class CityscapeVisibilityToggle : MonoBehaviour
{
    [Header("City")]
    [SerializeField] private Transform cityscapeRoot;

    [Header("Skybox")]
    [SerializeField] private Material citySkyboxMaterial;
    [SerializeField] private Material defaultSkyboxMaterial;

    [Header("Lighting")]
    [SerializeField] private Light directionalLight;

    private Renderer[] cachedRenderers;
    private Quaternion originalLightRotation;
    private Quaternion hiddenLightRotation;
    private bool lightRotationInitialized;

    private void Awake()
    {
        CacheRenderers();
        CacheLightRotations();
    }

    private void CacheRenderers()
    {
        if (cityscapeRoot == null)
        {
            Debug.LogWarning("[CityscapeVisibilityToggle] Cityscape root is not assigned.", this);
            cachedRenderers = System.Array.Empty<Renderer>();
            return;
        }

        cachedRenderers = cityscapeRoot.GetComponentsInChildren<Renderer>(true);
        Debug.Log($"[CityscapeVisibilityToggle] Cached {cachedRenderers.Length} renderers under {cityscapeRoot.name}.", this);
    }

    private void CacheLightRotations()
    {
        if (directionalLight == null)
        {
            Debug.LogWarning("[CityscapeVisibilityToggle] Directional light is not assigned.", this);
            return;
        }

        originalLightRotation = directionalLight.transform.rotation;
        hiddenLightRotation = originalLightRotation * Quaternion.Euler(0f, 180f, 0f);
        lightRotationInitialized = true;

        Debug.Log($"[CityscapeVisibilityToggle] Cached light rotations. Original: {originalLightRotation.eulerAngles}, Hidden: {hiddenLightRotation.eulerAngles}", this);
    }

    public void SetCityscapeVisible(bool isVisible)
    {
        if (cachedRenderers == null || cachedRenderers.Length == 0)
        {
            CacheRenderers();
        }

        foreach (Renderer r in cachedRenderers)
        {
            if (r != null)
                r.enabled = isVisible;
        }

        ApplySkybox(isVisible);
        ApplyDirectionalLightRotation(isVisible);

        Debug.Log($"[CityscapeVisibilityToggle] Cityscape visibility set to {isVisible}.", this);
    }

    private void ApplySkybox(bool isVisible)
    {
        Material targetSkybox = isVisible ? citySkyboxMaterial : defaultSkyboxMaterial;

        if (targetSkybox == null)
        {
            Debug.LogWarning("[CityscapeVisibilityToggle] Target skybox material is not assigned.", this);
            return;
        }

        RenderSettings.skybox = targetSkybox;
        DynamicGI.UpdateEnvironment();
    }

    private void ApplyDirectionalLightRotation(bool isVisible)
    {
        if (directionalLight == null)
        {
            Debug.LogWarning("[CityscapeVisibilityToggle] Directional light is not assigned.", this);
            return;
        }

        if (!lightRotationInitialized)
        {
            CacheLightRotations();
        }

        directionalLight.transform.rotation = isVisible ? originalLightRotation : hiddenLightRotation;

        Debug.Log($"[CityscapeVisibilityToggle] Directional light rotation set to {directionalLight.transform.eulerAngles}", this);
    }
}