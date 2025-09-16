using UnityEngine;

public class Ability : MonoBehaviour, IInteract
{
    private string AbilityName = "Ability";
    private BeeAI bee;
    public void Interact(GameObject interactor)
    {
        //bee.AbilityTrigger();
        Destroy(this.transform);
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
}
