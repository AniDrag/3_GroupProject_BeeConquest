using UnityEngine;
using System.IO;
using UnityEditor.Overlays;
/// <summary>
/// I should be on a game manager OBJ that is not destroyed... yess
/// </summary>
public class Save_System : MonoBehaviour {

    [SerializeField]private string playerName = "Tom"; // folder name and then maybe saves idk
    
    private const string fileName = "_rxe.dlr";
    private SaveData currentSave;
    
    public void Save()
    {
        GameSave gameSave = new GameSave() { playerLevel = 0, playerName = "Thomas", inventory = new string[] { "Sword", "helmet"} };

        string json = JsonUtility.ToJson(gameSave);
        Debug.Log(json);
        File.WriteAllText(Application.dataPath + "/Bin/" + playerName+fileName, json);
    }
    public void Load()
    {
        if(File.Exists(Application.dataPath + "/Bin/" + playerName + fileName))
        {
            string json = File.ReadAllText(Application.dataPath + "/Bin/" + playerName + fileName);
            GameSave gameSave = JsonUtility.FromJson<GameSave>(json);
            
            Debug.Log("Player Name: " + gameSave.playerName + ", Level: " + gameSave.playerLevel);
        }
        else
        {
            Debug.Log("NoSaveFile");
        }
    }

    public void LoadAllSaves()
    {
        string[] files = Directory.GetFiles(Application.dataPath + "/Bin/","*" + fileName);
        foreach (string file in files)
        {
            string json = File.ReadAllText(file);
            GameSave save = JsonUtility.FromJson<GameSave>(json);

            //getPrefab and load all datat here. Easy peasy
        }
    }

    public void LoadSaveData()
    {
        // passes its file name
    }
}
public class GameSave
{
    public string playerName;
    public int playerLevel;
    public int ID;
    public Vector3 time;// x = hours, y = min, z = sec
    // Organise and make all ur game data here
    public string[] inventory;
}
