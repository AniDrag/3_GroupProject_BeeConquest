using UnityEngine;
using System.Collections.Generic;
public class ConvertingBay : MonoBehaviour, IInteract
{
    [Header ("----- DATA -----")]
    [SerializeField] private Transform  RegistrationZone;
    [SerializeField] private PlayerCore registeredPlayer;
    [SerializeField] private InteractionType interactionType = InteractionType.OnKeyPress;
    [SerializeField] string interactionBayName = "Unregistered";
    [SerializeField] private int systemLevel = 1;
    bool activateCollection;

    List<BasicBee> convertingBees = new List<BasicBee>  ();
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
            convertingBees = registeredPlayer.DepositPollin();
            interactionBayName = "Fermenting...";
            registeredPlayer.visualsUI.interactedItemText.text = interactionBayName;
            activateCollection = true;
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
        interactionBayName = "Ferment pollin?";
        registeredPlayer.visualsUI.interactedItemText.text = interactionBayName;
        Debug.Log("Unsetting the bool");
        registeredPlayer.isConvertingPollen = false;
    }

    void GeneratePollin()
    {
        long convertionAmount = 0;
        foreach (var bee in convertingBees)
        {
            if (bee.atDestination)
                convertionAmount += (bee.GetBeeDex * bee.GetBeeStr);
            Debug.Log($"Bee {bee.name} is at destination: {bee.atDestination}, and location is {bee.transform.position}, whereas the home is {bee.homeCoordinates}, current bool is {registeredPlayer.isConvertingPollen}");
        }


        int count = registeredPlayer.allBees.Count;
        const long CAP = 100_000_000L;

        double d = convertionAmount * 10;
        long convertAmount = (double.IsInfinity(d) || d >= CAP) ? CAP : (long)d;

        Game_Manager.instance.ConvertPolinToHoney(convertAmount * systemLevel, registeredPlayer.playerID);

        if (registeredPlayer.GetCurrentPollin <= 0)
            registeredPlayer.isConvertingPollen = false;

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
