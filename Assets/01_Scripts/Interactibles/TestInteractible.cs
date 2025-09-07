using UnityEngine;

public class TestInteractible : MonoBehaviour, Iinteract
{
    public string interactionText = "Test Object";

    public void Interact()
    {
        Debug.Log("Door opened!");
    }

    public string GetInteractionText() => interactionText + "Press E to interact";
}
