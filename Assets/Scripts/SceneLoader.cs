using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadScene1()
    {
        SceneManager.LoadScene("CityScape");
    }

    public void LoadScene2()
    {
        SceneManager.LoadScene("Room");
    }
}