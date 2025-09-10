using UnityEngine;
using System.Collections.Generic;
using System;
public class CollectionData
{
    public float collectAmount;
    public int playerID;
    public int fieldCellID;
    public FieldGenerator field;
    public float triggerTime; // when it finishes
}
public class PlayerServerData
{
    public int playerID;
    public Transform target;
    public Vector3 lastKnownPosition;
    public PlayerCore playerCore;
    public List<BeeCore> playerBees = new List<BeeCore>();
    public List<BeeAI> playerBeesTwo = new List<BeeAI>();
    public FieldGenerator currentField;// trigger this
}
public class Game_Manager : MonoBehaviour
{

    public static Game_Manager instance;

    //-------------------
    //      Player & beee variables
    //-------------------
    public Dictionary<int,PlayerServerData> players = new Dictionary<int, PlayerServerData>();
    private List<CollectionData> collectionDatas = new List<CollectionData>();

    //-------------------
    //      Field data
    //-------------------
    private List<FieldGenerator> serverFields = new List<FieldGenerator>();
    public static event Action<float> OnFixedTick;

    private Dictionary<int, FieldCell> fieldCells = new Dictionary<int, FieldCell>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject); // optional, if you want it persistent
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private int beeCount;
    void Start()
    {
        GameObject beeHolder = GameObject.Find("beeholder");
        foreach (Transform child in beeHolder.transform)
        {
            BeeCore beeCore = child.GetComponent<BeeCore>();
            if (beeCore != null)
            {
                beeCount++;

                players[0].playerBees.Add(beeCore);
            }
        }

        Debug.Log("Added: " + beeCount + " bees");

    }

    private float beeStateUpdateInterval = 3f;        // seconds between calls
    private float beeRareTimer = 0f;
    private float beeNextRareTime = 0f;

    private float fieldStateUpdateInterval = 1.5f;
    private float fieldRareTimer = 0f;
    private float fieldNextRareTime = 0f;
    private void FixedUpdate()
    {
        beeRareTimer += Time.fixedDeltaTime;
        fieldRareTimer += Time.fixedDeltaTime;
        if (beeRareTimer >= beeNextRareTime)
        {
            beeRareTimer = 0f;
            beeNextRareTime = Mathf.Max(0.01f, beeStateUpdateInterval);

            foreach (int player in players.Keys)
            {
                float distance = Vector3.Distance(Game_Manager.instance.players[player].lastKnownPosition, Game_Manager.instance.players[player].target.position);
                //Debug.Log(distance);
                if (distance > 4)
                {
                    players[player].lastKnownPosition = players[player].target.position;
                    foreach (BeeCore bee in players[player].playerBees)
                    {
                        bee.CatchPlayer();
                    }
                }
            }

        }

        if (fieldRareTimer >= fieldNextRareTime)
        {
            float dt = fieldRareTimer;
            fieldRareTimer = 0f;
            fieldNextRareTime = Mathf.Max(0.01f, fieldStateUpdateInterval);
            // Invoke every subscribed tick updater
            for (int i = 0; i < collectionDatas.Count; i++)
            {
                if (collectionDatas[i].triggerTime <= Time.time)
                {
                    CollectPolinTrigger(collectionDatas[i]);
                }
            }
            OnFixedTick?.Invoke(dt);
        }
    }
    #region Field Data and cells
    public void OnBuffExpired(FieldCell cell, FieldBuff buff)
    {
        Debug.Log($"Buff {buff.type} expired on cell {cell.name}");
        // TODO: Sync with server, update UI, etc.
    }


    /// <summary>
    /// When the timer is 0 it triggers the collection data, transfers polin to player,
    /// And sends an update to the FieldCell so that the cell can check if it should update visualy
    /// That is taken care of by the cell it self, since the durability value is the same for all 
    /// clients we dont need anything else from it.
    /// </summary>
    public void CollectPolinTrigger(CollectionData data)
    {
        FieldGenerator generator = data.field;
        var cell = generator.GetCellById(data.fieldCellID);
        int pollin = Mathf.RoundToInt(cell.PollinMultiplier * data.collectAmount);
        PlayerCore player = players[data.playerID].playerCore;
        if (player == null) Debug.LogWarning("no player core found");
        player.AddPollin(pollin);
        cell.DecreaseDurability(data.collectAmount);
        collectionDatas.Remove(data);
    }
    // Decrese durability and send only that to the world. and buffs. regen happens localy and is sinced anyways since its doen on time.time durability increases unanimously.
   
    /// <summary>
    /// This function is called by a bee when it is ready to collect a new polin.
    /// This function handles asigning a Call for collecting polin and details sorounding that process.
    /// Server has info of where the bee is so we also send that info so no inconsistencies occure
    /// </summary>
    /// <param name="bee"></param>
    //public void BeeCollectionRequest(BeeAI bee)
    //{
    //    // get a Field cell that is nere player
    //    // get the bee 
    //    PlayerServerData player = players[bee.GetPlayerID];
    //    FieldCell field = GetPositionToFieldCell(player.target.position);
    //    bee.MoveTo(field.transform.position);
    //    //float travelTime = Vector3.Distance(allBeesOnServer[bee], field.transform.position); // some calculation for time idk
    //    CollectionData data = new CollectionData() {
    //        collectAmount = bee.GetCollectionStrenght,
    //        playerID = player.playerID,
    //        fieldCellID = field.GetID,
    //       // triggerTime = travelTime,
    //    };
    //    // All logic and info exchange here
    //    collectionDatas.Add(bee,data);
    //}
    public void BEE_PollinCollectionRequest(BeeAI bee, float collectionTime)
    {
        Debug.Log("Requesting field location from GM");
        FieldGenerator generator = players[bee.parentID].currentField;
        var cell = generator.GetRandomCellInRadius(players[bee.parentID].target.position,5);
        if (cell != null)
        {
            bee.SetDestination(cell.transform.position);

            CollectionData data = new CollectionData()
            {
                collectAmount = bee.collectionStrength,
                playerID = bee.parentID,
                fieldCellID = cell.ID,
                field = generator,
                triggerTime = collectionTime,
            };
            collectionDatas.Add(data);
        }
        else
        {
            Debug.Log("no cell found");
        }
    }
    public void BEE_IdleMoveRequest(BeeAI bee)
    {
        //Debug.Log("Requesting Idle movemen location from GM");
        Transform player = players[bee.parentID].target;
        Vector3 randomPosition = GetRandomPointInAnnulusXZ(player.position, 0.5f, 5f);
        //Debug.Log("SERVER: " + randomPosition);
        bee.SetDestination(randomPosition);
    }
    public void BEE_PlayerRequestForBeeToFollowPlayer(BeeAI bee)
    {
        //Debug.Log("Requesting player  location from GM");
        bee.SetDestination(players[bee.parentID].target.position);
    }

    public void BeeMovementRequest(BeeCore bee)
    {
        Transform player = players[bee.GetPlayerID].target;
        Vector3 randomPosition = GetRandomPointInAnnulusXZ(player.position, 0.5f, 5f);
        bee.MoveTo(randomPosition);
    }

    Vector3 GetRandomPointInAnnulusXZ(Vector3 center, float minR, float maxR)
    {
        float r = Mathf.Sqrt(UnityEngine.Random.value * (maxR * maxR - minR * minR) + minR * minR);
        float theta = UnityEngine.Random.value * Mathf.PI * 2f;
        Vector2 dir = new Vector2(Mathf.Cos(theta), Mathf.Sin(theta));
        return center + new Vector3(dir.x * r, 0f, dir.y * r);
    }

    /// <summary>
    /// Finction in progress. It is ment to get a random point around the player
    /// then it should find the closest fieldCell and asign it  as the destination for the bee.
    /// </summary>
    /// <param name="serchPivot"></param>
    /// <returns></returns>
    FieldCell GetPositionToFieldCell(Vector3 serchPivot)
    {
        // get player position, check which cells are in range
        return new FieldCell();
    }

    #endregion
    public void JoinServer(int ID, PlayerServerData data)
    {
        if (!players.ContainsKey(ID)) players.Add(ID, data);
        else players[ID].target.GetComponent<PlayerCore>().SpawnPlayerBees(players[ID].playerBeesTwo);
    }
    public void LeaveServer(int ID)
    {
        players.Remove(ID);
    }

    public void AsignFieldToServer(FieldGenerator generator)
    {
        serverFields.Add(generator);
    }
    public void AsignCurrentFieldToPlayer(int player, FieldGenerator field)
    {
        players[player].currentField = field;
        players[player].currentField = field;
    }
    public void ExitCurrentFieldFromPlayer(PlayerCore player)
    {
        players[player.playerID].currentField = null;
        player.currentField = null;
    }
}
