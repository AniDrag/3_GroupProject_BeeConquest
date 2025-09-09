using UnityEngine;
using System.Collections.Generic;
public class PlayerCore : MonoBehaviour
{
    //private string playerName = string.Empty;
    private int playerID = 0;

    private int polinStorage;

    private List<BeeCore> wariorBees = new List<BeeCore> ();
    private List<BeeCore> defenderBees = new List<BeeCore>();
    private List<BeeCore> workerBees = new List<BeeCore>();

    private Dictionary<int, List<BeeCore>> beeGroups = new Dictionary<int, List<BeeCore>>();// this is a dictionary of 3 squads of warrior and defender bees the player can asign adn then manipulate

    private void Awake()
    {
        PlayerServerData data = new PlayerServerData() {playerID = this.playerID, target = this.transform };
        Game_Manager.instance.JoinServer(playerID,data);
    }

    public int AddPollin(int amount) => polinStorage += amount;

    // Controling bees
}
