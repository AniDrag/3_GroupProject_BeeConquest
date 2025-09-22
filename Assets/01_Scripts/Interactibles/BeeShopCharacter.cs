using UnityEngine;

public class BeeShopCharacter : MonoBehaviour, IInteract
{
    public BeeShop beeShop;

    [SerializeField] private string InteractText;

    public void Interact(GameObject interactor)
    {
        beeShop = interactor.gameObject.GetComponent<PlayerCore>().visualsUI.shopPanel.GetComponent<BeeShop>();
        beeShop.gameObject.SetActive(true);
        beeShop.GetDataFromInteractor(interactor.GetComponent<PlayerCore>());
        Camera.main.GetComponent<PlayerCamera>().disableCamRotation = false;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    public void DeInteract(GameObject interactor)
    {
        beeShop.gameObject.SetActive(false);
        beeShop = null;
        Camera.main.GetComponent<PlayerCamera>().disableCamRotation = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public string GetInteractionText() => InteractText;

    public bool CanInteract(GameObject interactor) => interactor.GetComponent<PlayerCore>() != null;
    public InteractionType Type() => InteractionType.OnKeyPress;
}
