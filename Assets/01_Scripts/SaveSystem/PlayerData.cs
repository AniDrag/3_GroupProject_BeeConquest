using UnityEngine;
[System.Serializable]
/// <summary>
/// Set all Data that is stored by the player
///  this is a preset and will need edeting for future refrences
/// </summary>
public class PlayerData
{
    public string playerName;
    public int playerLevel;
    public int playerHealth;
    public int playerStamina;
    public int playerMana;

    // Stats?
    public int vitality;
    public int endurance;
    public int strenght;
    public int focus;
    public int luck;

    public float[] playerLocation;

    public PlayerData (PlayerData loadData/*Player player <-- example, insert draw from source where the data is going to be pulled from*/)
    {
        playerName = loadData.playerName;// example
        playerLevel = loadData.playerLevel;
        playerHealth = loadData.playerHealth;
        playerStamina = loadData.playerStamina;
        playerMana = loadData.playerMana;

        vitality = loadData.vitality;
        endurance = loadData.endurance;
        strenght = loadData.strenght;
        focus = loadData.focus;
        luck = loadData.luck;

        playerLocation = loadData.playerLocation;
    } 

}
