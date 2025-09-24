using UnityEngine;

public class BeeShopCharacter : MonoBehaviour, IInteract
{
    //public BeeShop beeShop;

    [SerializeField] private string InteractText;
    [SerializeField] private Transform beeSpawnerTrans;
    [SerializeField] private Transform HiveTransform;

    public void Interact(GameObject interactor)
    {
        Debug.Log("Triggered shop");
        UI_Visuals ui = interactor.gameObject.GetComponent<PlayerCore>().visualsUI;
        ui.UI_IInteractToggleShop(false);
        ui.beeShopPanel.GetComponent<BeeShop>().beeSpawner = beeSpawnerTrans;
        ui.beeShopPanel.GetComponent<BeeShop>().cellHolder = HiveTransform;
    }

    public void DeInteract(GameObject interactor)
    {
        Debug.Log("removed from shop");
        interactor.gameObject.GetComponent<PlayerCore>().visualsUI.UI_IInteractToggleShop(true);
    }
    public string GetInteractionText() => InteractText;

    public bool CanInteract(GameObject interactor) => interactor.GetComponent<PlayerCore>() != null;
    public InteractionType Type() => InteractionType.OnKeyPress;
}
