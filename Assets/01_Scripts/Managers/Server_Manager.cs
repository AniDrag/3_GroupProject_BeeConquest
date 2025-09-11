using System;
using System.Collections.Generic;
using UnityEngine;

public class Server_Manager : MonoBehaviour
{
    public static Server_Manager instance;

    private Queue<object> requestQueue = new Queue<object>();
    private float serverUpdateInterval = 1.5f;
    private float serverRareTimer = 0f;
    private float serverNextRareTime = 0f;
    public static event Action<float> OnFixedTick;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake() { if (instance == null) instance = this; else Destroy(gameObject); DontDestroyOnLoad(gameObject); }

    public Dictionary<int, PlayerServerData> players = new Dictionary<int, PlayerServerData>();

    private void FixedUpdate()
    {
        serverRareTimer += Time.fixedDeltaTime;
        if (serverRareTimer >= serverNextRareTime)
        {
            float dt = serverRareTimer;
            serverRareTimer = 0f;
            serverNextRareTime = Mathf.Max(0.01f, serverUpdateInterval);
            // Invoke every subscribed tick updater
            HandleRequest(requestQueue.Dequeue());
            OnFixedTick?.Invoke(dt);
        }
    }

    public void SendRequest(object request)
    {
        requestQueue.Enqueue(request);
    }

    private void HandleRequest(object request)
    {
        switch (request)
        {
            case PollinCollectionData pollin:
                HandlePollin(pollin);
                break;

            case CombatData combat:
                HandleCombat(combat);
                break;

            case EnemyAIData enemyData:
                HandleEnemy(enemyData);
                break;

            default:
                Debug.LogWarning("[Server] Unknown request type");
                break;
        }
    }
    private void HandlePollin(PollinCollectionData data)
    {
        // server decides if pollin is collectible
        if (data.field != null)
        {
            Debug.Log($"[Server] Pollin collected at Cell {data.CellID} (Client {data.ID})");
            //data.field.CollectCell(data.CellID, data.durabilityDamage);
            BroadcastToAll(data);
        }
    }

    private void HandleCombat(CombatData data)
    {
        // server applies damage
        if (data.enemy != null)
        {
            //data.enemy.TakeDamage(data.damage);
            Debug.Log($"[Server] Bee {data.bee} hit {data.enemy} for {data.damage}");
            BroadcastToAll(data);
        }
    }

    private void HandleEnemy(EnemyAIData data)
    {
        if (data.enemy != null)
        {
            //data.enemy.SetPosition(data.position.position);
            Debug.Log($"[Server] Enemy {data.enemy.name} moved (authoritative)");
            BroadcastToAll(data);
        }
    }

    private void BroadcastToAll(object data)
    {
       // foreach (var client in connectedClients.Values)
       // {
       //     client.ReceiveUpdate(data);
       // }
    }
    

    public void JoinServer(int playerID, PlayerServerData data)
    {
        if (!players.ContainsKey(playerID))
        {
            players.Add(playerID, data);
            Debug.Log($"[Server] Player {playerID} joined server.");
        }
    }

    // ================= Server Requests =================

    public void Server_RequestAddPollin(int playerID, int amount)
    {
        if (players.TryGetValue(playerID, out var data))
        {
            //data.pollinStorage += Mathf.Max(0, amount); // server validates
            //data.client.OnPollinUpdated(data.pollinStorage);
            //Debug.Log($"[Server] Player {playerID} pollin updated: {data.pollinStorage}");
        }
    }

    public void Server_RequestSpawnBees(int playerID, List<BeeAI> bees)
    {
        if (players.TryGetValue(playerID, out var data))
        {
            // server owns authoritative bee list
            //data.playerBeesTwo.AddRange(bees);
            //data.client.OnBeesSpawned(data.playerBeesTwo);
            //Debug.Log($"[Server] Player {playerID} spawned bees: {bees.Count}");
        }
    }

    public void Server_RequestBeeFollow(int playerID, BeeAI bee)
    {
        if (players.TryGetValue(playerID, out var data))
        {
            if (data.playerBeesTwo.Contains(bee))
            {
               // bee.SetFollowTarget(data.target);
               // Debug.Log($"[Server] Player {playerID} ordered bee to follow.");
            }
        }
    }

    // ================= Example Server Reconciliation =================
    public void Server_UpdatePlayerPosition(int playerID, Vector3 predictedPos)
    {
        if (players.TryGetValue(playerID, out var data))
        {
            // server can correct if prediction is off
           // data.target.position = predictedPos;
           // data.client.OnServerPositionUpdate(data.target.position);
        }
    }
}
public class ClientData
{
    public string clientName;
    public string ID;
    public Dictionary<string, int> currency = new Dictionary<string, int>();
    public List<BeeAI> allBees = new List<BeeAI>();
    public FieldGenerator currentField;
}
public class PollinCollectionData
{
    public int ID;
    public int durabilityDamage;
    public FieldGenerator field;
    public int CellID;
    public float executionTime;
}

public class CombatData
{
    public EnemyCore enemy;
    public int damage;
    public BeeAI bee;// gain xp
    public PlayerCore player;// gains rewards is sent to enemy
}
public class EnemyAIData
{
    public int ID;
    public EnemyCore enemy;
    public Transform position;
}
public class DataPacket
{
    public int playerID;
    Queue<object> requestQueue = new Queue<object>();

}