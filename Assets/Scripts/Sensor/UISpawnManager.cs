using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class UISpawnFromRay : MonoBehaviour
{
    [SerializeField] private XRRayInteractor rayInteractor;
    [SerializeField] private GameObject prefabToSpawn;
    [SerializeField] private Transform rayOrigin;
    [SerializeField] private float fallbackDistance = 1.0f;
    [SerializeField] private Vector3 spawnOffset = Vector3.zero;

    public void SpawnFromRay()
    {
        if (prefabToSpawn == null || rayInteractor == null)
            return;

        Transform origin = rayOrigin != null ? rayOrigin : rayInteractor.transform;
        Vector3 spawnPosition = origin.position + origin.forward * fallbackDistance + spawnOffset;
        Quaternion spawnRotation = origin.rotation;

        Instantiate(prefabToSpawn, spawnPosition, spawnRotation);
    }
}