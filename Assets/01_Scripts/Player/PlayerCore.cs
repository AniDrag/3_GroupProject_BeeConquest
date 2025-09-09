using UnityEngine;

public class PlayerCore : MonoBehaviour
{
    //private string playerName = string.Empty;
    private int playerID = 0;

    private int polinStorage;

    private void Awake()
    {
        PlayerServerData data = new PlayerServerData() {playerID = this.playerID, target = this.transform };
        Game_Manager.instance.JoinServer(data);
    }

    public int AddPollin(int amount) => polinStorage += amount;
}
