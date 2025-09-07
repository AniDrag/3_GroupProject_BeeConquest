using UnityEngine;

/// <summary>
/// All saves are managed here
/// </summary>
public static class SaveSystem {

   // public static void SavePlayerData()// from here we would have PlayerData player
   // {
   //     string path = Application.persistentDataPath + "/sfpData.sfp";// random name for the save file
   //
   //     BinaryFormatter formatter = new BinaryFormatter();
   //     FileStream stream = new FileStream(path, FileMode.Create);
   //
   //     PlayerData dataPlayer = new PlayerData();// inside it would say player and the data would write it self
   //     formatter.Serialize(stream, dataPlayer);
   //     stream.Close();
   // }
   //
   // public static PlayerData LoadPlayerData()// from here we would have PlayerData player
   // {
   //     string path = Application.persistentDataPath + "/sfpData.sfp";// random name for the save file
   //     if (File.Exists(path))
   //     {
   //         BinaryFormatter formatter = new BinaryFormatter();
   //         FileStream stream = new FileStream(path, FileMode.Open);
   //         PlayerData dataPlayer = new PlayerData();
   //         dataPlayer = formatter.Deserialize(stream) as PlayerData;
   //         stream.Close();
   //         return dataPlayer;
   //     }
   //     else
   //     {
   //         Debug.LogError("There is no file at file path --> " + path + "   Please check if saved game or other error");
   //         return null;
   //     }
   // }
   //
   // public static void SaveGameData()// from here we would have PlayerData player
   // {
   //     string path = Application.persistentDataPath + "/sfpData.sfp";// random name for the save file
   //
   //     BinaryFormatter formatter = new BinaryFormatter();
   //     FileStream stream = new FileStream(path, FileMode.Create);
   //
   //     GameData dataGame = new GameData();// inside it would say player and the data would write it self
   //     formatter.Serialize(stream, dataGame);
   //     stream.Close();
   // }
   //
   // public static GameData LoadGameData()// from here we would have PlayerData player
   //{
   //    string path = Application.persistentDataPath + "/sfpData.sfp";// random name for the save file
   //    if (File.Exists(path))
   //    {
   //        BinaryFormatter formatter = new BinaryFormatter();
   //        FileStream stream = new FileStream(path, FileMode.Open);
   //        GameData dataGame = new GameData();
   //        dataGame = formatter.Deserialize(stream) as GameData;
   //        stream.Close();
   //        return dataGame;
   //    }
   //    else
   //    {
   //        Debug.LogError("There is no file at file path --> " + path + "   Please check if saved game or other error");
   //        return null;
   //    }
   //}

}
