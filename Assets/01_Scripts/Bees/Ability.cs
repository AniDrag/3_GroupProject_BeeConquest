using UnityEngine;

public class Ability : MonoBehaviour, IInteract
{
    private string AbilityName = "Ability";
    private BeeAI bee;
    float time;
    [SerializeField, Range(5,20)] float duration = 10;
    public void Interact(GameObject interactor)
    {
        //bee.AbilityTrigger();
        bee.TriggerAbilityLogic(bee, interactor.GetComponent<PlayerCore>());
        Destroy(gameObject);
    }
    public void DeInteract(GameObject interactor)
    {

    } //uselsess
    public string GetInteractionText() => AbilityName;
    public bool CanInteract(GameObject interactor) => interactor.GetComponent<PlayerCore>() != null;
    public InteractionType Type() => InteractionType.WhenInRange;

    public void SetAbilityData(BeeAI parentBee, string setName)
    {
        AbilityName = setName;
        bee = parentBee;
    }
    private void Start()
    {
        time = Time.time + duration;
    }
    private void Update()
    {
        if(time < Time.time)Destroy(gameObject);
    }
}
