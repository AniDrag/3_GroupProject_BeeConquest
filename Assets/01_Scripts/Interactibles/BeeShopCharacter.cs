using UnityEngine;

public class BeeShopCharacter : MonoBehaviour, IInteract
{
    //public BeeShop beeShop;

    [SerializeField] private string InteractText;

    public void Interact(GameObject interactor)
    {
        Debug.Log("Triggered shop");
        interactor.gameObject.GetComponent<PlayerCore>().visualsUI.UI_BTNToggleShop();
    }

    public void DeInteract(GameObject interactor)
    {
        Debug.Log("removed from shop");
        interactor.gameObject.GetComponent<PlayerCore>().visualsUI.UI_BTNToggleShop();
    }
    public string GetInteractionText() => InteractText;

    public bool CanInteract(GameObject interactor) => interactor.GetComponent<PlayerCore>() != null;
    public InteractionType Type() => InteractionType.OnKeyPress;
}
