using UnityEngine;

public class ItemPickup : MonoBehaviour, Iinteract
{
    public string itemName;

    public string GetInteractionText() => $"(E) Pick up {itemName}";
    public InteractionType Type() => InteractionType.OnKeyPress;
    public void Interact(GameObject interactor)
    {
        Debug.Log($"Picked up {itemName}");
        Destroy(gameObject);
    }
    public void DeInteract(GameObject interactor) { }
    public bool CanInteract(GameObject interactor) => interactor.GetComponent<PlayerCore>() != null;
}
    