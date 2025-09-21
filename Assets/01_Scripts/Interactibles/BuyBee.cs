using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;

[System.Serializable]
public class BeeTypesForTheShopDontUseThisPlease
{
    public string BeeName;
    public long Cost;
    public GameObject Bee;
}
public class BuyBee : MonoBehaviour, IInteract
{
    [SerializeField] private string InteractText;
    [SerializeField] private List<BeeTypesForTheShopDontUseThisPlease> bees = new List<BeeTypesForTheShopDontUseThisPlease>();
    [SerializeField] private int CellStartPrice = 1000;
    [SerializeField] private float CellPriceMultiplier = 4.23f;
    private int currentCellPrice;

    [SerializeField] private GameObject beeSelectionUI;
    [SerializeField] private TextMeshProUGUI currentBees;
    [SerializeField] private TextMeshProUGUI currentCells;
    [SerializeField] private TextMeshProUGUI buyCellButton;

    private int cellCounter;

    private GameObject playerInteractor;
    private PlayerCore playerCore;

    void Start()
    {
        currentCellPrice = CellStartPrice;
        cellCounter = 1;
        currentCells.text = "Cells: 1";
        currentBees.text = "Bees: 1";
        buyCellButton.text = currentCellPrice.ToString();
    }
    public void Interact(GameObject interactor)
    {
        beeSelectionUI.gameObject.SetActive(true);
        playerInteractor = interactor;
        playerCore = playerInteractor.GetComponent<PlayerCore>();
    }

    public void Buy(int index)
    {
        Debug.Log($"You want to by {bees[index].BeeName}, and you have to pay {bees[index].Cost}, also {cellCounter >= playerCore.allBees.Count}, and {playerCore.currentHoneyAmount >= bees[index].Cost}");
        if (cellCounter > playerCore.allBees.Count && playerCore.currentHoneyAmount >= bees[index].Cost)
        {
            Debug.Log("Suck");
            playerCore.BuyBee(bees[index].Bee, transform);
            currentBees.text = "Bees: " + playerCore.allBees.Count;
            playerCore.RemoveHoney(bees[index].Cost);
        }
    }

    public void BuyCell()
    {
        if (playerCore.currentHoneyAmount >= currentCellPrice)
        {
            cellCounter++;
            playerCore.RemoveHoney(currentCellPrice);
            currentCellPrice *= (int)CellPriceMultiplier;
            currentCells.text = "Cells: " + cellCounter.ToString();
            buyCellButton.text = currentCellPrice.ToString();
        }

    }
    public void DeInteract(GameObject interactor)
    {

    }
    public string GetInteractionText() => InteractText;

    public bool CanInteract(GameObject interactor)
    {
        return false;
    }
    public InteractionType Type()
    {
        return InteractionType.OnKeyPress;
    }
}
