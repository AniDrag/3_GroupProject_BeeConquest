using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BeeShop : MonoBehaviour
{
    [Header("---------- Refrences ----------")]
    [SerializeField] private PlayerCore _player;
    [Space]
    [SerializeField]private ShopItem buyBees;
    [SerializeField]private ShopItem upgradeInventory;
    [SerializeField]private ShopItem upgradeCellCount;
    [Space]
    [SerializeField] private TMP_Text cellsUsedText;
    [SerializeField] private TMP_Text cellsEmptyText;

    [Header("---------- Bee Collection ----------")]
    [SerializeField] Transform beePanel;
    [SerializeField] private List<BeeTypesForTheShopDontUseThisPlease> beesForSale = new List<BeeTypesForTheShopDontUseThisPlease>();
    [SerializeField] GameObject ShopItemPRF;

    bool displayedBees;
    private int _emptyCells;
    private int _usedCells;
    private int _totalCells;

    public void GetDataFromInteractor()
    {
        if (!displayedBees)
        {
            displayedBees = true;
            foreach(var beee in beesForSale)
            {               
                var newbee = Instantiate(ShopItemPRF, beePanel);
                ShopItem beeData = newbee.GetComponent<ShopItem>();
                beeData.AsignData(beee.BeeName, beee.Cost);
            }
        }
        _usedCells = _player.allBees.Count;
        _totalCells = _player.ownedCellsAmount;
        _emptyCells = _totalCells - _usedCells;
    }
    public void UpgradeInventory()
    {

    }
    public void UpgradeCellAmount()
    {

    }
}
