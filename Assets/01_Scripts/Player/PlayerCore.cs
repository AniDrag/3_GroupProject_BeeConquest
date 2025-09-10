using UnityEngine;
using System.Collections.Generic;
public class PlayerCore : MonoBehaviour
{
    //private string playerName = string.Empty;
    public int playerID { get; private set; } = 0;

    [SerializeField] private int polinStorage;

    private Dictionary<int, List<BeeCore>> beeGroups = new Dictionary<int, List<BeeCore>>();// this is a dictionary of 3 squads of warrior and defender bees the player can asign adn then manipulate
    private List<BeeAI> playerBees = new List<BeeAI> ();
    [SerializeField] public FieldGenerator currentField;
    private void Awake()
    {
        Debug.Log(playerID);
        PlayerServerData data = new PlayerServerData() {playerID = this.playerID, target = this.transform };
        Game_Manager.instance.JoinServer(playerID,data);
    }

    public int AddPollin(int amount) => polinStorage += amount;

    public void SpawnPlayerBees(List<BeeAI> beeList)
    {
        foreach (var bee in beeList)
        {
            playerBees.Add(bee);
            bee.SetMyParent(this);
            Game_Manager.instance.players[playerID].playerBeesTwo.Add(bee);
            // bee nees a skin and a type
            //spawn bee and parent give the be the proper bee data and player data.
        }
    }

    // Controling bees

    public void SetBeeStatesToFollow(BeeAI orderedBee)
    {
        Game_Manager.instance.BEE_PlayerRequestForBeeToFollowPlayer(orderedBee);

    }
    public void FocusTargetedEnemy()
    {

    }
    public void MoveToTargetedSpot()
    {

    }
    public void StartComandingBees()
    {

    }
}
