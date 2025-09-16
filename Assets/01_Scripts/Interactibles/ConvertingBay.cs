using UnityEngine;

public class ConvertingBay : MonoBehaviour, IInteract
{
    [Header ("----- DATA -----")]
    [SerializeField] private Transform  RegistrationZone;
    [SerializeField] private PlayerCore registeredPlayer;
    [SerializeField] private InteractionType interactionType = InteractionType.OnKeyPress;
    [SerializeField] string interactionBayName = "Unregistered";
    private int systemLevel = 1;
    bool activateCollection;
    #region Tick stuff
    float beeRareTimer;
    float beeNextRareTime;
    float beeStateUpdateInterval = 1;
    #endregion

    public bool CanInteract(GameObject interactor) => registeredPlayer == null || interactor.GetComponent<PlayerCore>() == registeredPlayer;
    public InteractionType Type() => interactionType;
    public string GetInteractionText() => interactionBayName;
    public void Interact(GameObject interactor)
    {
        if(registeredPlayer == null)
        {
            Debug.Log("Registering: " + interactor.name);
            registeredPlayer = interactor.GetComponent<PlayerCore>();
            interactionBayName = "Ferment pollin?";
            registeredPlayer.visualsUI.interactedItemText.text = interactionBayName;
            return;
        }
        else
        {
            Debug.Log($" {interactor.name} requested pollin fermentation");
            registeredPlayer.DepositPollin(this.transform);
            activateCollection = false;
        }

    }
    public void DeInteract(GameObject interactor)
    {
        if (registeredPlayer == null ) return;
        Debug.Log($" {interactor.name} stopped pollin fermentation");
        activateCollection = false; 
        beeRareTimer = 0;
        beeNextRareTime = 0;
        registeredPlayer.CleareComands();
    }

    void GeneratePollin()
    {
        Game_Manager.instance.ConvertPolinToHoney(1000 * systemLevel, registeredPlayer.playerID);
    }

    private void FixedUpdate()
    {
        if(!activateCollection)return;
        beeRareTimer += Time.fixedDeltaTime;
        if (beeRareTimer >= beeNextRareTime)
        {
            beeRareTimer = 0f;
            beeNextRareTime = Mathf.Max(0.3f, beeStateUpdateInterval);
            Debug.Log($" Fermenting pollin");
            GeneratePollin();
        }
    }
}
