using System.Collections;
using UnityEngine;

public class ConvertingBay : MonoBehaviour, Iinteract
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
            registeredPlayer = interactor.GetComponent<PlayerCore>();
            interactionBayName = "Ferment pollin?";
            return;
        }
        else
        {
            registeredPlayer.DepositPollin(this.transform);
            activateCollection = false;
        }

    }
    public void DeInteract(GameObject interactor)
    {
        if (registeredPlayer == null ) return;
        activateCollection = false; 
        beeRareTimer = 0;
        beeNextRareTime = 0;
        registeredPlayer.CleareComands();
    }

    void GeneratePollin()
    {
        Game_Manager.instance.ConvertPolinToHoney(10000 * systemLevel, registeredPlayer.playerID);
    }

    private void FixedUpdate()
    {
        if(!activateCollection)return;
        beeRareTimer += Time.fixedDeltaTime;
        if (beeRareTimer >= beeNextRareTime)
        {
            float dt = beeRareTimer;
            beeRareTimer = 0f;
            beeNextRareTime = Mathf.Max(0.3f, beeStateUpdateInterval);
            GeneratePollin();
        }
    }
}
