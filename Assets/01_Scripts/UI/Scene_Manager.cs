using AniDrag.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Scene_Manager : MonoBehaviour
{
    public static Scene_Manager instance;
    [Tooltip("Prepared for a loading screen")]
    public GameObject LoadSceneObj;
    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
    }
    public void SCENE_LoadScene(int sceneIndex)
    {
        //Add a safety Check
        SceneManager.LoadSceneAsync(sceneIndex);
    }
    [Button("Reload Scene", ButtonSize.Small, 20,.3f,.5f,.9f,SdfIconType.ToggleOn)]
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

