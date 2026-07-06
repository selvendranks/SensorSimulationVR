using UnityEngine;

public class ToggleControlViewUI : MonoBehaviour
{
    [SerializeField] private GameObject canvasObject;

    public void ToggleCanvas()
    {
        if (canvasObject == null)
            canvasObject = gameObject;

        canvasObject.SetActive(!canvasObject.activeSelf);
    }
}