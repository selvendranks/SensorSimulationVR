using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SceneReseter : MonoBehaviour
{
    [SerializeField] private Key resetKey = Key.R;

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current[resetKey].wasPressedThisFrame)
        {
            ResetCurrentScene();
        }
    }

    public void ResetCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
}