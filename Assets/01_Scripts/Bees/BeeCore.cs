using UnityEngine;

public class BeeCore : Stats
{
    public enum BeeClass { Worker, Attacker, Defender }
    public enum BeeAtribute { Red, Blue, Green, Dark, Light }
    public enum State { Idle, MovingToTarget, CollectingPolin, Attacking }

    [Header("Settings")]
    [SerializeField, Range(1, 100)] float collectionStrength = 5f;

    public BeeClass beeClass;
    public BeeAtribute beeAtribute;
    public State beeState;

    public Vector3 Destination { get; private set; }
    private Transform enemy;
    private bool _atDestination;
    private int playerID;
    public int GetPlayerID => playerID;
    public float GetCollectionStrenght => collectionStrength;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        switch (beeState)
        {
            case State.Idle: IdleBehaviour(); break;
            case State.CollectingPolin: CollectingPolinBehaviour(); break;
            case State.Attacking: AttackingBehaviour(); break;
        }

        MovementBehaviour();
    }
/// <summary>
/// This is called by the player when they change location tyep. or the player detects an enemy
/// </summary>
/// <param name="state"> 1 = Idle, 2 = Collect polin, 3 = Attacking</param>
    public void SetState(int state)
    {
        switch (state)
        {
            case 1:
                beeState = State.Idle; break;
            case 2:
                beeState = State.CollectingPolin; break;
            case 3:
                beeState = State.Attacking; break;
        }
    }
    // Public API
    /// <summary>
    /// Get info from server so no cheating is possible.
    /// Works with movement behaviour and moves the be to destination. Once there depending on the 
    /// state it should be, it does one of the fillowin states
    /// </summary>
    /// <param name="currentPosition"></param>
    /// <param name="targetPosition"></param>
    public void MoveTo(Vector3 currentPosition, Vector3 targetPosition)
    {
        transform.position = currentPosition;
        Destination = targetPosition;
        _atDestination = false;
        beeState = State.MovingToTarget;
    }

    /// <summary>
    /// Consistantly called for bee movemant at all times
    /// </summary>
    private void MovementBehaviour()
    {
        if (!_atDestination &&Destination != Vector3.zero)
        {
            transform.position = Vector3.MoveTowards(transform.position, Destination, Agility * Time.deltaTime);

            if (Vector3.Distance(transform.position, Destination) < 0.1f)
            {
                _atDestination = true;
                Destination = Vector3.zero;
                Game_Manager.instance.BeeCollectionRequest(this);
            }
        }
    }
    void IdleBehaviour()
    {
        if(_atDestination) {

            PING_DestinationNerePlayer();
        }
        // random point around player. Always the same height
    }
    void CollectingPolinBehaviour()
    {
        if (_atDestination)
        {
            //animation trigger and 
            PING_DestinationToFieldCell();
        }
        // send request to server for location of polin flower, amount it can collect, time to travel to location
        // Get location
        // travel there
        // go to idle for a bit and then get new location and repeat
    }
    void AttackingBehaviour()
    {
        if (_atDestination)
        {
            if (enemy != null)
            {
                PING_DestinationToEnemy();
            }
            else { }// Deal damage to target
            //animation trigger and 
        }
        // scan for closes target and deal damage to it
        // send data to whitch target we delt damage to server and how much
    }


    void PING_DestinationNerePlayer()
    {
        _atDestination = false;
        // send a ping to server
    }
    void PING_DestinationToFieldCell()
    {
        _atDestination = false;
        // send a ping to server
    }
    void PING_DestinationToEnemy()
    {
        _atDestination = false;
        // send a ping to server
    }
}
