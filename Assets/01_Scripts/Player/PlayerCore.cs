using UnityEngine;
using System.Collections.Generic;
using TMPro;
using AniDrag.Utility;
public class PlayerCore : MonoBehaviour
{
    [Header("----- UI refrences -----")]
    [SerializeField] private TMP_Text honneyStorage;
    [SerializeField] GameObject floatingNumPrefab;
    [Header("----- Bee Data -----")]
    [SerializeField] int spawnNumPerClick = 75;
    [SerializeField] public BeeAI[] testBee;
    private Dictionary<int, List<BeeAI>> beeGroups = new Dictionary<int, List<BeeAI>>();// this is a dictionary of 3 squads of warrior and defender bees the player can asign adn then manipulate
    private List<BeeAI> playerBees = new List<BeeAI> ();
    [SerializeField] GameObject BeePRF;

    [Header("----- Field Data -----")]
    [SerializeField] public FieldGenerator currentField;

    [Header("----- Inventory Data -----")]
    [SerializeField] private long polinStorage;

    #region Getters
    public int playerID { get; private set; } = 0;

    #endregion

    private void Awake()
    {
        
        foreach (var bee in testBee)
        {
            playerBees.Add(bee);
        }
        beeGroups.Add(1,playerBees);

        foreach(var bee in playerBees)
        {
            bee.SetMyParent(this);
        }

        PlayerServerData data = new PlayerServerData(playerID, transform, this, playerBees) { };

        Game_Manager.instance.JoinServer(playerID,data);
    }

    
    private void FixedUpdate()
    {
        //if (playerBees.Count > 0) Debug.Log(playerBees.Count + " Bee amount from Player");
        for (int i = 0; i< playerBees.Count;i++)
        {
            float distance = Vector3.Distance(transform.position, playerBees[i].transform.position);
            if (distance > 5 && 
                playerBees[i].stateMachine.currentState != playerBees[i].chaseState && 
                playerBees[i].stateMachine.currentState != playerBees[i].pollinCollectionState)
            {
                //Debug.Log("player requested bee to follow DISTANCE:" + distance);
                Game_Manager.instance.BEE_PlayerRequestForBeeToFollowPlayer(playerBees[i]);
            }

        }
    }
    public void AddPollin(long amount, Vector3 position)
    {
        //Debug.Log("Player recived honney");
        polinStorage += amount; 
        honneyStorage.text = $"Honey: {polinStorage / 5}";
        GameObject go = Instantiate(floatingNumPrefab, position, Quaternion.identity);
        go.transform.position += Vector3.up * 1f;
        FloatingNumbers floatingNumber = go.GetComponent<FloatingNumbers>();
        floatingNumber.Initialize(amount); // offset above cell

    }
    [Button("SpawnBees", ButtonSize.Medium, 0, 0, 0, 1, SdfIconType.None)]
    public void SpawnBees()
    {
        for (int i = 0; i < spawnNumPerClick; i++)
        {
            GameObject newBee = Instantiate(BeePRF);
            BeeAI bee = newBee.GetComponent<BeeAI>();
            bee.SetMyParent(this);
            playerBees.Add(bee);
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
    public void StopComandingBees() { }
    public void FollowTarget(BeeAI bee, Transform target) {
        
    }
}
