using UnityEngine;

public class BeeAI : MonoBehaviour
{
    public enum BeeStates { Idle, moving, collecting, attacking }
    public BeeStates state;

    public Transform player;
    public Vector3 moveToPoint;
    float distanceToTarget;
    float distanceToPlayer;
    float timeBetweenAttacking;
    bool destinationSet;
    bool attacked;
    bool atDestination;
    bool hasEnemy;
    bool isOnFeild;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
        distanceToTarget = Vector3.Distance(transform.position, moveToPoint);

        if (hasEnemy)// Depending on what the player incounters the state changes. if there is an enemy we should attack it, if no enemy and we are on a field then we collect polin else we fly around the player
        {
            state = BeeStates.attacking;
        }
        else if (isOnFeild)
        {
            state = BeeStates.collecting;
        }
        else
        {
            state = BeeStates.Idle;
        }
        
    }
    void MovemantLogic()
    {
        if (distanceToPlayer > 10)
        {
            FollowPlayer();
        }
        else
        {
            MoveToPosition();// temp vector

            if (hasEnemy && atDestination)
            {
                AttackTarget();
            }
            if (atDestination)
            {
                CollectPollin();
            }
        }
    }

    void FollowPlayer()
    {
        state = BeeStates.moving;
        moveToPoint = player.position;
    }
    void MoveToPosition()
    {
        if(!destinationSet) FindDestination();return;
        transform.position = Vector3.MoveTowards(transform.position, moveToPoint, 2 * Time.deltaTime);
    }
    void FindDestination()
    {
        //Ping server for destination
        switch (state)
        {
            case BeeStates.Idle:
                //Ping idle position search
                break;
            case BeeStates.collecting:
                //Ping Find a field cell
                break;
            case BeeStates.attacking:
                // Find a enemy target
                break;
        }
    }
    void AttackTarget()
    {
        state = BeeStates.attacking;
    }
    void CollectPollin()
    {

    }
}
