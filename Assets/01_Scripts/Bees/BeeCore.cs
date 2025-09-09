using UnityEngine;

public class BeeCore : Stats
{
    public enum BeeClass// probably better to just make a seperate script
    {
        Worker,
        Attacker,
        Defender
    }
    public enum BeeAtribute
    {
        Red,
        Blue,
        Green,
        Dark,
        Light
    }

    public enum State
    {
        Idle,
        CollectingPolin,
        Attacking,
    }

    [Header("Settings")]
    [SerializeField, Range(1, 100)] float collectionStrenght;
    public BeeClass beeClass;
    public BeeAtribute beeAtribute;
    public State beeState;
    public Transform player;
    public Vector3 destinaton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (beeState)
        {
            case State.Idle:
                IdleBehaviour(); break;
            case State.CollectingPolin:
                CollectingPolinBehaviour(); break;
            case State.Attacking:
                AttackingBehaviour(); break;
        }
    }

    ///<summary>
    /// Bee movement. is a location to location move transform there with jsut an actual movement
    /// has atimer to go to a random location around the player
    /// has 3 states is passive, is on field, attacks
    /// 
    /// </summary>
    /// 

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

    void IdleBehaviour()
    {
        // random point around player. Always the same height
    }
    void CollectingPolinBehaviour()
    {
        // send request to server for location of polin flower, amount it can collect, time to travel to location
        // Get location
        // travel there
        // go to idle for a bit and then get new location and repeat
    }
    void AttackingBehaviour()
    {
        // scan for closes target and deal damage to it
        // send data to whitch target we delt damage to server and how much
    }

    void MovementBehaviour()// Maybe this? we use it whenever the bee moves. Gets movement instructions with this.
    {

    }
}
