using UnityEngine;
using System.Collections.Generic;
public class CollectionData
{
    public float collectAmount;
    public int playerID;
    public int fieldCellID;
    public float duration;
    public float triggerTime;// when it should triger
}
public class PlayerServerData
{
    public int playerID;
    public Transform target;
    public PlayerCore playerCore;
}
public class Game_Manager : MonoBehaviour
{

    public static Game_Manager instance;

    // List of cells that have drability
    // List of cells that do not have durability
    // List of stuff that holds data for triggering animations and collection of polin (cell ID, player ID)
    // List of users with only their ID and transform
    private List<PlayerServerData> players = new List<PlayerServerData>();
    private List<CollectionData> collectionDatas = new List<CollectionData>();
    private List<FieldCell> fieldCells = new List<FieldCell>();

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
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // some kind of timer that is here that collects data each second it checks if anything in the list is done
        for (int i = 0; i < collectionDatas.Count; i++)
        {
            if (collectionDatas[i].triggerTime <= Time.time)
            {
                CollectPolinTrigger(collectionDatas[i]);
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
    /// When the timer is 0 it triggers the collection data
    /// </summary>
    void CollectPolinTrigger(CollectionData data)
    {
        FieldCell cell = fieldCells[data.fieldCellID];
        int pollin = Mathf.RoundToInt(cell.GetPolinMultiplyer * data.collectAmount);
        players[data.playerID].playerCore.AddPollin(pollin);
        fieldCells[data.fieldCellID].DecreseDurability((int)data.collectAmount);
    }
    // Decrese durability and send only that to the world. and buffs. regen happens localy and is sinced anyways since its doen on time.time durability increases unanimously.
    public void BeeCollectionRequest(Vector3 position, Vector3 playerPos)
    {
        CollectionData data = new CollectionData();
        // All logic and info exchange here
        collectionDatas.Add(data);
    }
    #endregion
    public void JoinServer(PlayerServerData data)
    {
        players.Add(data);
    }
    public void LeaveServer(PlayerServerData data)
    {
        players.Remove(data);
    }
}
