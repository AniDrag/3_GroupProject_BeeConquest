using System.Collections.Generic;
using AniDrag.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class BeeTypesForTheShopDontUseThisPlease
{
    public string BeeName;
    public long Cost;
    public Sprite beeSprite;
    public GameObject Bee;
}
[System.Serializable]
public class BeeFood
{
    public string materialName;
    public long Cost;
    public Sprite materialSprite;
    public long heldXP;
}
public class BeeShop : MonoBehaviour
{
    [Header("---------- Refrences ----------")]
    [SerializeField] private PlayerCore _player;
    [SerializeField] private Transform beeSpawner;
    [SerializeField] GameObject ShopItemPRF;
    [Space]
    [SerializeField]private ShopItem upgradeInventory;//[SerializeField]private ShopItem buyBees;
    [SerializeField]private ShopItem upgradeCellCount;
    [Space]
    [SerializeField] private TMP_Text cellsUsedText;

    [Header("---------- Bee Collection ----------")]
    [SerializeField] Transform beeContent;
    [SerializeField] private List<BeeTypesForTheShopDontUseThisPlease> beesForSale = new List<BeeTypesForTheShopDontUseThisPlease>();

    [Header("---------- Materials Collection ----------")]
    [SerializeField] Transform foodContent;
    [SerializeField] private List<BeeFood> foodForSale = new List<BeeFood>();


    [Header("---------- Cell price settings ----------")]
    [SerializeField] private long cellStartPrice = 1000;
    [SerializeField] private float cellMulti = 4.23f;
    private long _currentCellPrice;

    [Header("---------- Inventory uprade price settings ----------")]
    [SerializeField] private long inventoryUpgradeStartPrice = 1000;
    [SerializeField] private float inventoryPriceMulti = 1.98f;
    private long _currentInventoryPrice;

    private int _emptyCells;
    private int _usedCells;
    private int _totalCells;
    private void Awake()
    {
        upgradeInventory.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() => UpgradeInventory());
        upgradeCellCount.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() => UpgradeCellAmount());
    }
    [Button]
    public void GetDataFromInteractor(PlayerCore player)
    {
        _player = player;

        //SetUpBees();
        UpdateCellConter();

    }
    public void UpgradeInventory()
    {
        if (_player.currentHoneyAmount >= _currentInventoryPrice)
        {
            _player.UpgradeMaxPollinStorage();
            _player.RemoveHoney(_currentInventoryPrice);
            _currentInventoryPrice = inventoryUpgradeStartPrice + (long)Mathf.Pow((inventoryUpgradeStartPrice * ((float)_player.pollinStorageLevel * inventoryPriceMulti)), _player.pollinStorageLevel / 20f);
            upgradeCellCount.updatePrice(_currentCellPrice);
            // Get the ShopItem that upgrades cells and update its price?
            //currentCells.text = "Cells: " + cellCounter.ToString();
            //buyCellButton.text = currentCellPrice.ToString();
        }
    }
    public void UpgradeCellAmount()
    {
        if (_player.currentHoneyAmount >= _currentCellPrice)
        {
            _totalCells++;
            _player.RemoveHoney(_currentCellPrice);
            _currentCellPrice = cellStartPrice + (long)Mathf.Pow((cellStartPrice * ((float)_totalCells * cellMulti)), _totalCells / 20f);
            UpdateCellConter();
            upgradeCellCount.updatePrice(_currentCellPrice);
            // Get the ShopItem that upgrades cells and update its price?
            //currentCells.text = "Cells: " + cellCounter.ToString();
            //buyCellButton.text = currentCellPrice.ToString();
        }
    }
    public void BuyBee(int index)
    {
        long price = beesForSale[index].Cost;
        Debug.Log($"You want to by {beesForSale[index].BeeName}, and you have to pay {price}, you have {_emptyCells} empty cells, and do we have enough money: {_player.currentHoneyAmount >= price}");
        if (_emptyCells > 0 && _player.currentHoneyAmount >= price)
        {
            Debug.Log("Suck");
            _player.BuyBee(beesForSale[index].Bee, beeSpawner);
            cellsUsedText.text = "Bees: " + _player.allBees.Count;
            _player.RemoveHoney(price);
            UpdateCellConter();
        }
    }
    public void BuyFood(int index)
    {
        long price = foodForSale[index].Cost;
        Debug.Log($"You want to by {foodForSale[index].materialName}, and you have to pay {price}, do we have enough money: {_player.currentHoneyAmount >= price}");
        if (_player.currentHoneyAmount >= price)
        {
            _player.AddFoodItem(foodForSale[index]);
            _player.RemoveHoney(price);
        }
    }

    private void UpdateCellConter()
    {
        _usedCells = _player.allBees.Count;
        _totalCells = _player.ownedCellsAmount;
        _emptyCells = _totalCells - _usedCells;
        cellsUsedText.text = $"Cells space: {_usedCells} out off {_totalCells}";
    }
    //obsolete
    public void SetAction(Button button,UnityEngine.Events.UnityAction action)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    // BUTTON SHIT OK
    [Button]
    void SetUpBees()
    {
        for (int i = beesForSale.Count - 1; i == 0; i--) 
        {
            Destroy(beeContent.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < beesForSale.Count; i++)
        {
            GameObject newbee = Instantiate(ShopItemPRF, beeContent);
            ShopItem beeData = newbee.GetComponent<ShopItem>();
            beeData.AsignData(beesForSale[i].BeeName, beesForSale[i].Cost, beesForSale[i].beeSprite);
            beeData.button.onClick.AddListener(() => BuyBee(i));
        }
    }
    [Button]
    void SetUpmaterials()
    {
        for (int i = beesForSale.Count - 1; i == 0; i--)
        {
            Destroy(foodContent.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < foodForSale.Count; i++)
        {
            GameObject newMaterial = Instantiate(ShopItemPRF, beeContent);
            ShopItem materialData = newMaterial.GetComponent<ShopItem>();
            materialData.AsignData(foodForSale[i].materialName, foodForSale[i].Cost, foodForSale[i].materialSprite);
            materialData.button.onClick.AddListener(() => BuyBee(i));
        }
    }
}
