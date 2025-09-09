using UnityEngine;
using System.Collections.Generic;
public class CollectionData
{
    public float collectAmount;
    public int playerID;
    public int fieldCellID;
    public float triggerTime; // when it finishes
}
public class PlayerServerData
{
    public int playerID;
    public Transform target;
    public Vector3 lastKnownPosition;
    public PlayerCore playerCore;
    public List<BeeCore> playerBees = new List<BeeCore>();
}
public class Game_Manager : MonoBehaviour
{

    public static Game_Manager instance;

    // List of cells that have drability
    // List of cells that do not have durability
    // List of stuff that holds data for triggering animations and collection of polin (cell ID, player ID)
    // List of users with only their ID and transform
    public Dictionary<int,PlayerServerData> players = new Dictionary<int, PlayerServerData>();
    private List<CollectionData> collectionDatas = new List<CollectionData>();
    private Dictionary<string, List<FieldCell>> fields = new Dictionary<string, List<FieldCell>>();
    private Dictionary<int, FieldCell> fieldCells = new Dictionary<int, FieldCell>();
    //private Dictionary<BeeCore, Vector3> allBeesOnServer = new Dictionary<BeeCore, Vector3>();


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

    // Update is called once per frame
    void Update()
    {
        // some kind of timer that is here that collects data each second it checks if anything in the list is done
        for (int i = collectionDatas.Count - 1; i >= 0; i--)
        {
            if (collectionDatas[i].triggerTime <= Time.time)
            {
                CollectPolinTrigger(collectionDatas[i]);
                collectionDatas.RemoveAt(i);
            }
        }
    }


    private float stateUpdateInterval = 3f;        // seconds between calls
    private float rareTimer = 0f;
    private float nextRareTime = 0f;
    private void FixedUpdate()
    {
        rareTimer += Time.fixedDeltaTime;
        if (rareTimer >= nextRareTime)
        {
            rareTimer = 0f;
            nextRareTime = Mathf.Max(0.01f, stateUpdateInterval);

            foreach (int player in players.Keys)
            {
                float distance = Vector3.Distance(Game_Manager.instance.players[player].lastKnownPosition, Game_Manager.instance.players[player].target.position);
                Debug.Log(distance);
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
    void CollectPolinTrigger(CollectionData data)
    {
        FieldCell cell = fieldCells[data.fieldCellID];
        int pollin = Mathf.RoundToInt(cell.GetPolinMultiplyer * data.collectAmount);
        players[data.playerID].playerCore.AddPollin(pollin);
        fieldCells[data.fieldCellID].DecreseDurability((int)data.collectAmount);
    }
    // Decrese durability and send only that to the world. and buffs. regen happens localy and is sinced anyways since its doen on time.time durability increases unanimously.
   
    /// <summary>
    /// This function is called by a bee when it is ready to collect a new polin.
    /// This function handles asigning a Call for collecting polin and details sorounding that process.
    /// Server has info of where the bee is so we also send that info so no inconsistencies occure
    /// </summary>
    /// <param name="bee"></param>
    public void BeeCollectionRequest(BeeCore bee)
    {
        // get a Field cell that is nere player
        // get the bee 
        PlayerServerData player = players[bee.GetPlayerID];
        FieldCell field = GetPositionToFieldCell(player.target.position);
        bee.MoveTo(field.transform.position);
        //float travelTime = Vector3.Distance(allBeesOnServer[bee], field.transform.position); // some calculation for time idk
        CollectionData data = new CollectionData() {
            collectAmount = bee.GetCollectionStrenght,
            playerID = player.playerID,
            fieldCellID = field.GetID,
           // triggerTime = travelTime,
        };
        // All logic and info exchange here
        collectionDatas.Add(data);
    }
    
    public void BeeMovementRequest(BeeCore bee)
    {
        Transform player = players[bee.GetPlayerID].target;
        Vector3 randomPosition = GetRandomPointInAnnulusXZ(player.position, 0.5f, 5f);
        bee.MoveTo(randomPosition);
    }

    Vector3 GetRandomPointInAnnulusXZ(Vector3 center, float minR, float maxR)
    {
        float r = Mathf.Sqrt(Random.value * (maxR * maxR - minR * minR) + minR * minR);
        float theta = Random.value * Mathf.PI * 2f;
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
        players.Add(ID,data);
    }
    public void LeaveServer(int ID)
    {
        players.Remove(ID);
    }
}
