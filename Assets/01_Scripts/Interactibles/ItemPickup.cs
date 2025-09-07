using UnityEngine;

public class ItemPickup : MonoBehaviour, Iinteract
{
    public string itemName;

    public string GetInteractionText() => $"(E) Pick up {itemName}";

    public void Interact()
    {
        Debug.Log($"Picked up {itemName}");
        Destroy(gameObject);
    }
}
