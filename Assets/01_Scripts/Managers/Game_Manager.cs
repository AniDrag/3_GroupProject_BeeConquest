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
    public Transform transform;
    public Vector3 lastKnownPosition;
    public PlayerCore playerCore;
    public List<BeeCore> playerBees = new List<BeeCore>();
    public List<BeeAI> playerBeesTwo = new List<BeeAI>();
    public FieldGenerator currentField;// trigger this
    public PlayerServerData(int PlayerID, Transform PlayerTransform, PlayerCore Core, List<BeeAI> PlayerBees)
    {
        playerID = PlayerID;
        transform = PlayerTransform;
        playerCore = Core;
        playerBeesTwo = PlayerBees;
    }
}
public class Game_Manager : MonoBehaviour
{
    // ───────────── INSTANCE ─────────────
    public static Game_Manager instance;

    //-------------------
    //      Player & beee variables
    //-------------------
    public Dictionary<int, PlayerServerData> players = new Dictionary<int, PlayerServerData>();
    private List<CollectionData> collectionDatas = new List<CollectionData>();

    //-------------------
    //      Field data
    //-------------------
    private List<FieldGenerator> serverFields = new List<FieldGenerator>();
    public static event Action<float> OnFixedTick;

    //-------------------
    //      Pollen
    //-------------------
    private ServerLabelAgregator serverLabelAgregator;


    // ───────────── SINGELTON PATERN ─────────────
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject); // optional, if you want it persistent
        serverLabelAgregator = new ServerLabelAgregator(clusterRadius: 5f, processIntervalSeconds: .2f, minLabelThreshold: 16f);
        serverLabelAgregator.Start();
    }

    private void OnDestroy()
    {
        serverLabelAgregator?.Stop();
        serverLabelAgregator?.Dispose();
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
    #region fixed tick variables
    private float beeStateUpdateInterval = 3f;        // seconds between calls
    private float beeRareTimer = 0f;
    private float beeNextRareTime = 0f;

    private float fieldStateUpdateInterval = .1f;
    private float fieldRareTimer = 0f;
    private float fieldNextRareTime = 0f;
    #endregion
    private void FixedUpdate()
    {
        beeRareTimer += Time.fixedDeltaTime;
        fieldRareTimer += Time.fixedDeltaTime;

        // ───────────── BEE REQUEST UPDATE FIXED TICK RATE ─────────────
        if (beeRareTimer >= beeNextRareTime)
        {
            beeRareTimer = 0f;
            beeNextRareTime = Mathf.Max(0.01f, beeStateUpdateInterval);

            foreach (int player in players.Keys)
            {
                float distance = Vector3.Distance(Game_Manager.instance.players[player].lastKnownPosition, Game_Manager.instance.players[player].transform.position);
                //Debug.Log(distance);
                if (distance > 4)
                {
                    players[player].lastKnownPosition = players[player].transform.position;
                    foreach (BeeCore bee in players[player].playerBees)
                    {
                        bee.CatchPlayer();
                    }
                }
            }
        }

        // ───────────── FIELD FIXED TICK RATE ─────────────
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
            //// IF WE HAVE ACTUAL SERVER, USE THIS:
            //var aggLabels = damageAggregator?.DrainResults();
            //if (aggLabels != null && aggLabels.Count > 0)
            //{
            //    foreach (var lab in aggLabels)
            //    {
            //        // Build compact payload; send to relevant clients (optimize by distance)
            //        var payload = new AggLabelPayload
            //        {
            //            color = (int)lab.color,
            //            totalAmount = lab.totalAmount,
            //            clusterX = lab.clusterX,
            //            clusterY = lab.clusterY,
            //            clusterZ = lab.clusterZ,
            //            playerX = lab.playerX,
            //            playerY = lab.playerY,
            //            playerZ = lab.playerZ,
            //            playerId = lab.representativePlayerId
            //        };

            //        Network_SendToAllClients_ShowAggregatedLabel(payload); // THIS IS THE PACKAGE WE SEND.
            //    }
            //}
            if (serverLabelAgregator != null)
            {
                var aggLabels = serverLabelAgregator.DrainResults();
                if (aggLabels != null && aggLabels.Count > 0)
                {
                    foreach (var lab in aggLabels)
                    {
                        // Convert to the expected types
                        long amount = (long)Mathf.Round(lab.totalAmount);
                        long honeyReceived = amount / 5;

                        Vector3 clusterPos = new Vector3(lab.clusterX, lab.clusterY, lab.clusterZ);

                        // Only call ShowPollinVisual on the representative player's PlayerCore.
                        // If representative player or playerCore is missing, skip showing.
                        if (lab.representativePlayerId >= 0 && players.TryGetValue(lab.representativePlayerId, out var repData) && repData.playerCore != null)
                        {
                            repData.playerCore.ShowPollinVisual(amount, clusterPos, lab.color, honeyReceived);
                        }
                        // else: intentionally do nothing (no FloatingLabelPool call)
                    }
                }
            }
        }
    }
    // ───────────── FIELD FUNCTIONS ─────────────
    #region Field Functions
    public void OnBuffExpired(FieldCell cell, FieldBuff buff)
    {
        Debug.Log($"Buff {buff.type} expired on cell {cell.name}");
        // TODO: Sync with server, update UI, etc.
    }
    public void AddBuff(FieldCell cell, FieldBuff buff)
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
        if (cell == null) return;

        int pollin = 0;
        float actualTaken = 0f;

        if (cell.CurrentDurability < data.collectAmount)
        {
            actualTaken = cell.CurrentDurability;
            pollin = Mathf.RoundToInt(cell.PollinMultiplier * cell.CurrentDurability);
            cell.DecreaseDurability(cell.CurrentDurability - 1);
        }
        else
        {
            actualTaken = data.collectAmount;
            pollin = Mathf.RoundToInt(cell.PollinMultiplier * data.collectAmount);
            cell.DecreaseDurability(data.collectAmount);
        }

        // Give resources to player but DO NOT spawn a visual here:
        PlayerCore player = players[data.playerID].playerCore;
        if (player == null) Debug.LogWarning("no player core found");
        else player.AddPollin(pollin, 0);

        // Enqueue damage for aggregator visual grouping (local)
        if (serverLabelAgregator != null && actualTaken > 0f)
        {
            var ev = new DamageEvent
            {
                sourcePlayerId = data.playerID,
                color = cell.Color,
                amount = actualTaken,
                worldX = cell.WorldPosition.x,
                worldY = cell.WorldPosition.y,
                worldZ = cell.WorldPosition.z,
                playerWorldX = players[data.playerID].transform.position.x,
                playerWorldY = players[data.playerID].transform.position.y,
                playerWorldZ = players[data.playerID].transform.position.z
            };
            serverLabelAgregator.EnqueueDamage(ev);
        }

        collectionDatas.Remove(data);
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
    #endregion
    // ───────────── BEE REQUESTS ─────────────
    #region Bee Requests
    /// <summary>
    /// This function is called by a bee when it is ready to collect a new polin.
    /// This function handles asigning a Call for collecting polin and details sorounding that process.
    /// Server has info of where the bee is so we also send that info so no inconsistencies occure
    /// </summary>
    /// <param name="bee"> the bee that requested this</param>
    /// <param name="collectionTime"> time it will take to collect polin</param>
    public void BEE_PollinCollectionRequest(BeeAI bee)
    {
        Debug.Log("Requesting field location from GM");
        FieldGenerator generator = players[bee.playerID].currentField;
        var cell = generator.GetRandomCellInRadius(players[bee.playerID].transform.position, 5);
        if (cell != null)
        {
            bee.SetDestination(cell.transform.position);

            // get travel (seconds) and total (travel + collection)
            float travel = bee.GetTravelTime(cell.transform.position); // travel only now
            float total = travel + bee.CollectionDuration;

            CollectionData data = new CollectionData()
            {
                collectAmount = bee.collectionStrength,
                playerID = bee.playerID,
                fieldCellID = cell.ID,
                field = generator,
                triggerTime = Time.time + total,   // <-- store absolute timestamp
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
        Transform player = players[bee.playerID].transform;
        Vector3 randomPosition = GetRandomPointInAnnulusXZ(player.position, 0.5f, 5f);
        //Debug.Log("SERVER: " + randomPosition);
        bee.SetDestination(randomPosition);
    }
    public void BEE_PlayerRequestForBeeToFollowPlayer(BeeAI bee, bool order = false)
    {
        //Debug.Log("Requesting player  location from GM");
        bee.SetDestination(players[bee.playerID].transform.position);
        bee.stateMachine.ChangeState(bee.chaseState);
    }
    public void BeeMovementRequest(BeeCore bee)
    {
        Transform player = players[bee.GetPlayerID].transform;
        Vector3 randomPosition = GetRandomPointInAnnulusXZ(player.position, 0.5f, 5f);
        bee.MoveTo(randomPosition);
    }

    #endregion
    // ───────────── PLAYER REQUESTS & FNCTIONS ─────────────
    #region Player Server Calls
    public void JoinServer(int ID, PlayerServerData data)
    {
        if (!players.ContainsKey(ID)) //IF NEW PLAYER
            players.Add(ID, data);
        //else // IF Already Registered 
        //    players[ID].transform.GetComponent<PlayerCore>().SpawnPlayerBees(players[ID].playerBeesTwo);
    }
    public void LeaveServer(int ID)
    {
        players.Remove(ID);// DESPAWN ALL PLAYER GAMEOBJECTS
    }


    #endregion
    // ───────────── MATH FNCTIONS ─────────────
    #region Math Functions
    Vector3 GetRandomPointInAnnulusXZ(Vector3 center, float minR, float maxR)
    {
        float r = Mathf.Sqrt(UnityEngine.Random.value * (maxR * maxR - minR * minR) + minR * minR);
        float theta = UnityEngine.Random.value * Mathf.PI * 2f;
        Vector2 dir = new Vector2(Mathf.Cos(theta), Mathf.Sin(theta));
        return center + new Vector3(dir.x * r, 0f, dir.y * r);
    }
    #endregion
    // ───────────── UNUSED FUNCTIONS ─────────────
    #region Unused Functions
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
}
