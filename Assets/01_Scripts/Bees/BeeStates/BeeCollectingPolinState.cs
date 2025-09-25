using UnityEngine;

public class BeeCollectingPolinState : BeeStates
{
    public BeeCollectingPolinState(BeeStateMachine StateMachine, BasicBee Bee) : base(StateMachine, Bee) { }
    private float nextCollectTime;
    bool spawnAbility;
    public override void EnterState() {
        bee.beeState = BeeState.Collecting;
        nextCollectTime = Time.time + bee.modedPollinCollectionSpeed;
        spawnAbility = Random.value < bee.modedSpawnTokenChance;
        //Debug.Log("Collected polin" + nextCollectTime);
        //stateMachine.animator.SetTrigger("Pollinating");
    }
    public override void ExitState() { }
    public override void LogicUpdate()
    {
        if (nextCollectTime >= Time.time) return;
        if (spawnAbility)
        {
            spawnAbility = false;
            bee.SpawnAbility();
        }
            bee.StateMachine.ChangeState(bee.moveingState);
            //bee.GetDestinationData();
    }
    public override void LateLogicUpdate() { }
    public override void FixedLogicUpdate() {
    }
    public override void AnimationTriggerEvent() { }//PlayerMovemant.AnimationTriggers triggerType) { }

}
