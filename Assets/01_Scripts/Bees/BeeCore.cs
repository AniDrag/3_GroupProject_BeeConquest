using System.Collections;
using UnityEngine;

public class BeeCore : Stats
{
    public enum BeeClass { Worker, Attacker, Defender }
    public enum BeeAtribute { Red, Blue, Green, Dark, Light }
    public enum State { Idle, MovingToTarget, CollectingPolin, Attacking, Chasing }

    [Header("Settings")]
    [SerializeField, Range(1, 100)] float collectionStrength = 5f;

    public BeeClass beeClass;
    public BeeAtribute beeAtribute;
    public State beeState;

    public Vector3 Destination { get; private set; }
    private Transform enemy;
    //private bool _atDestination;
    private int playerID;
    public int GetPlayerID => playerID;
    public float GetCollectionStrenght => collectionStrength;
    private float heightOffsetY = 0.4f;

    private float stateUpdateInterval = 3f;        // seconds between calls
    private float rareTimer = 0f;
    private float nextRareTime = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //if (Game_Manager.instance.players.ContainsKey(playerID)) 
        // Game_Manager.instance.players[playerID].playerBees.Add(this); // TEMP ADDING ALL BEES TO ONE PLAYER (THE CURRENT ONE)!!
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        rareTimer += Time.fixedDeltaTime;
        if (rareTimer >= nextRareTime)
        {
            rareTimer = 0f;
            nextRareTime = Mathf.Max(0.01f, stateUpdateInterval);
            AI();
            //Debug.Log("Update state bees");
        }



    }

    private void Update()
    {
        ClientSmoothMoving();
    }

    private void AI()
    {
        switch (beeState)
        {
            case State.Idle: IdleTransition(); break;
            case State.MovingToTarget: MovingBehaviour(); break;
            case State.CollectingPolin: CollectingPolinBehaviour(); break;
            case State.Attacking: AttackingBehaviour(); break;
            case State.Chasing: ClientSmoothMoving(); break;
        }
    }

    // ALEX
    // Like we call a method in every players bee to change the state. For example we have this in the playerCore:
    //OnTriggerEnter / Exit
    //depending on the trigger we update playerCurrentState, and do:  
    //foreach (var bee in playerBees)
    //{
    //    bee.SetState(playerCurrentState);
    //}
    // Then you don't have to encode it in an int, and just literally set the same state.
    /// <summary>
    /// This is called by the player when they change location tyep. or the player detects an enemy
    /// </summary>
    /// <param name="state"> 1 = Idle, 2 = Moving to target, 3 = Collect polin, 4 = Attacking</param>
    /// 
    public void SetState(int state)
    {
        switch (state)
        {
            case 1:
                beeState = State.Idle; break;
            case 2:
                beeState = State.MovingToTarget; break;
            case 3:
                beeState = State.CollectingPolin; break;
            case 4:
                beeState = State.Attacking; break;
            case 5:
                beeState = State.Chasing; break;
        }
    }

    public void CatchPlayer()
    {
        beeState = State.Chasing;
        MoveTo(Game_Manager.instance.players[playerID].transform.position);
        Debug.Log("I HAVE TO CATCH YOU");
        AI();
    }
    // Public API
    /// <summary>
    /// Get info from server so no cheating is possible.
    /// Works with movement behaviour and moves the be to destination. Once there depending on the 
    /// state it should be, it does one of the fillowin states
    /// </summary>
    /// <param name="currentPosition"></param>
    /// <param name="targetPosition"></param>
    public void MoveTo(Vector3 targetPosition)
    {
        Destination = targetPosition + new Vector3(Random.Range(-0.5f, 0.5f), heightOffsetY, Random.Range(-0.5f, 0.5f));
    }

    /// <summary>
    /// Consistantly called for bee movemant at all times
    /// </summary>
    private void ClientSmoothMoving()
    {
        if (Destination != Vector3.zero)
        {
            transform.position = Vector3.MoveTowards(transform.position, Destination, Agility * Time.deltaTime);

            if (Vector3.Distance(transform.position, Destination) < 0.1f)
            {
                Destination = Vector3.zero;
                //Game_Manager.instance.BeeCollectionRequest(this);
                if (beeState == State.Chasing)
                {
                    beeState = State.MovingToTarget;
                    AI();
                }
                else
                    beeState = State.Idle;
            }
        }
    }

    void IdleTransition()
    {
        if (Random.value > .95)
        {
            beeState = State.MovingToTarget;
        }
    }

    void MovingBehaviour()
    {
  
        Debug.Log("I REQUEST MOVEMENT POS");
        PING_DestinationNearPlayer();
        
        // random point around player. Always at the hight of the player (around the legs+-)
    }
    void CollectingPolinBehaviour()
    {
            //animation trigger and 
            PING_DestinationToFieldCell();
        
        // send request to server for location of polin flower, amount it can collect, time to travel to location
        // Get location
        // travel there
        // go to idle for a bit and then get new location and repeat
    }
    void AttackingBehaviour()
    {
      
            if (enemy != null)
            {
                PING_DestinationToEnemy();
            }
            else { }// Deal damage to target
            //animation trigger and 
        
        // scan for closes target and deal damage to it
        // send data to whitch target we delt damage to server and how much
    }


    void PING_DestinationNearPlayer()
    {
        Game_Manager.instance.BeeMovementRequest(this);

        // send a ping to server
    }
    void PING_DestinationToFieldCell()
    {
        // send a ping to server
    }
    void PING_DestinationToEnemy()
    {
        // send a ping to server
    }
}
