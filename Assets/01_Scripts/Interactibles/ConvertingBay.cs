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
    Coroutine _activeCorutine;

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
            _activeCorutine = StartCoroutine(GeneratePollin());

        }

    }
    public void DeInteract(GameObject interactor)
    {
        if (registeredPlayer == null && _activeCorutine !=null) return;
        StopCoroutine(GeneratePollin());
        registeredPlayer.CleareComands();
        _activeCorutine = null; 
    }

    IEnumerator GeneratePollin()
    {
        WaitForSeconds wait = new WaitForSeconds(1);
        Game_Manager.instance.ConvertPolinToHoney(10000 * systemLevel, registeredPlayer.playerID);
        yield return wait;
        StartCoroutine(GeneratePollin());
    }

}
