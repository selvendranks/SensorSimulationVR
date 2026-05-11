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

        Vector3 spawnPosition;
        Quaternion spawnRotation;

        RaycastHit hit;
        if (rayInteractor.TryGetCurrent3DRaycastHit(out hit))
        {
            spawnPosition = hit.point + spawnOffset;
            spawnRotation = Quaternion.LookRotation(hit.normal);
        }
        else
        {
            Transform origin = rayOrigin != null ? rayOrigin : rayInteractor.transform;
            spawnPosition = origin.position + origin.forward * fallbackDistance + spawnOffset;
            spawnRotation = origin.rotation;
        }

        Instantiate(prefabToSpawn, spawnPosition, spawnRotation);
    }
}