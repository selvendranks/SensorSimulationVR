using UnityEngine;
using UnityEngine.Rendering;

public class UnderWaterPostProcessing : MonoBehaviour
{
    [Header("Volume Reference")]
    [SerializeField] private Volume targetVolume;

    [Header("Volume Profiles")]
    [SerializeField] private VolumeProfile aboveWaterProfile;
    [SerializeField] private VolumeProfile underWaterProfile;

    [Header("Water Surface")]
    [SerializeField] private float waterSurfaceY = 0f;

    private bool wasUnderwater;

    private void Start()
    {
        wasUnderwater = transform.position.y < waterSurfaceY;
        ApplyProfile();
    }

    private void Update()
    {
        bool isUnderwater = transform.position.y < waterSurfaceY;

        if (isUnderwater != wasUnderwater)
        {
            wasUnderwater = isUnderwater;
            ApplyProfile();
        }
    }

    private void ApplyProfile()
    {
        if (targetVolume == null)
            return;

        targetVolume.sharedProfile = wasUnderwater ? underWaterProfile : aboveWaterProfile;
    }
}