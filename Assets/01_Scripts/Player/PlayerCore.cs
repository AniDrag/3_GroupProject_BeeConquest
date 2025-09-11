using UnityEngine;
using System.Collections.Generic;
public class PlayerCore : MonoBehaviour
{
    //private string playerName = string.Empty;
    public int playerID { get; private set; } = 0;

    [SerializeField] private int polinStorage;

    private Dictionary<int, List<BeeAI>> beeGroups = new Dictionary<int, List<BeeAI>>();// this is a dictionary of 3 squads of warrior and defender bees the player can asign adn then manipulate
    private List<BeeAI> playerBees = new List<BeeAI> ();
    [SerializeField] public FieldGenerator currentField;
    public BeeAI testBee;
    private void Awake()
    {
        playerBees.Add(testBee);
        beeGroups.Add(1,playerBees);
        PlayerServerData data = new PlayerServerData(playerID, transform, this, playerBees) { };
        Game_Manager.instance.JoinServer(playerID,data);
    }

    public int AddPollin(int amount) => polinStorage += amount;

    private void FixedUpdate()
    {
        for (int i = 0; i< playerBees.Count;i++)
        {
            if (Vector3.Distance(playerBees[i].transform.position, transform.position) < 5) continue;
            Debug.Log("player requested bee to follow");
            Game_Manager.instance.BEE_PlayerRequestForBeeToFollowPlayer(playerBees[i]);

        }
    }
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
        Game_Manager.instance.BEE_PlayerRequestForBeeToFollowPlayer(orderedBee,true);

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
