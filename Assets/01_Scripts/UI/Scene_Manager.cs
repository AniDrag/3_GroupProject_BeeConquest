using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene_Manager : MonoBehaviour
{
    [Tooltip("Prepared for a loading screen")]
    public GameObject LoadSceneObj;
    public void SCENE_LoadScene(int sceneIndex)
    {
        //Add a safety Check
        SceneManager.LoadSceneAsync(sceneIndex);
    }
    public void SCENE_ReloadScene()
    {
        int index = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadSceneAsync(index);
    }
    public void SCENE_QuitGame()
    {
        Application.Quit();
    }


}

