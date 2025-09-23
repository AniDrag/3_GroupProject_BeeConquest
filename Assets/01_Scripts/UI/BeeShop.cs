using AniDrag.Utility;
using System;
using System.Collections.Generic;
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
    public string foodName;
    public long Cost;
    public Sprite foodSprite;
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
        SetUpBees();
        SetUpFood();
        UpdateCellCounter();
    }
    [Button]
    public void GetDataFromInteractor(PlayerCore player)
    {
        _player = player;

        //SetUpBees();
        UpdateCellCounter();

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
        if (_player == null)
        {
            Debug.Log("No player here yet");
            return;
        }
        if (_player.currentHoneyAmount >= _currentCellPrice)
        {
            _totalCells++;
            _player.RemoveHoney(_currentCellPrice);
            _currentCellPrice = cellStartPrice + (long)Mathf.Pow((cellStartPrice * ((float)_totalCells * cellMulti)), _totalCells / 20f);
            _player.AddCell();
            UpdateCellCounter();
            upgradeCellCount.updatePrice(_currentCellPrice);
            // Get the ShopItem that upgrades cells and update its price?
            //currentCells.text = "Cells: " + cellCounter.ToString();
            //buyCellButton.text = currentCellPrice.ToString();
            Debug.Log($"You have {_player.ownedCellsAmount - _player.allBees.Count} free cells");
        }
    }
    public void BuyBee(int index)
    {
        UpdateCellCounter();
        long price = beesForSale[index].Cost;

        if (_emptyCells <= 0)
        {
            Debug.Log("No empty cells to buy a bee!");
            return;
        }

        if (_player.currentHoneyAmount < price)
        {
            Debug.Log("Not enough honey to buy this bee!");
            return;
        }

        Debug.Log($"Buying bee {beesForSale[index].BeeName} for {price} honey");
        _player.BuyBee(beesForSale[index].Bee, beeSpawner);
        _player.RemoveHoney(price);
        UpdateCellCounter();
    }
    public void BuyFood(int index)
    {
        long price = foodForSale[index].Cost;

        if (_player.currentHoneyAmount < price)
        {
            Debug.Log("Not enough honey to buy this food!");
            return;
        }

        Debug.Log($"Buying food {foodForSale[index].foodName} for {price} honey");
        _player.AddFoodItem(foodForSale[index]);
        _player.RemoveHoney(price);
    }

    private void UpdateCellCounter()
    {
        _usedCells = _player.allBees.Count;
        _totalCells = _player.ownedCellsAmount;
        _emptyCells = _totalCells - _usedCells;
        cellsUsedText.text = $"Cells space: {_usedCells} / {_totalCells}, empty {_emptyCells}";
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
        for (int i = beeContent.childCount - 1; i >= 0; i--)
            Destroy(beeContent.GetChild(i).gameObject);
        for (int i = 0; i < beesForSale.Count; i++)
        {
            GameObject newBee = Instantiate(ShopItemPRF, beeContent);
            ShopItem beeData = newBee.GetComponent<ShopItem>();
            beeData.AsignData(beesForSale[i].BeeName, beesForSale[i].Cost, beesForSale[i].beeSprite);

            int index = i; // capture index correctly
            beeData.button.onClick.AddListener(() => BuyBee(index));
        }
    }
    [Button]
    void SetUpFood()
    {
        // Clear previous food items
        for (int i = foodContent.childCount - 1; i >= 0; i--)
            Destroy(foodContent.GetChild(i).gameObject);

        for (int i = 0; i < foodForSale.Count; i++)
        {
            GameObject newMaterial = Instantiate(ShopItemPRF, foodContent);
            ShopItem materialData = newMaterial.GetComponent<ShopItem>();
            materialData.AsignData(foodForSale[i].foodName, foodForSale[i].Cost, foodForSale[i].foodSprite);

            int index = i; // capture index correctly
            materialData.button.onClick.AddListener(() => BuyFood(index));
        }
    }
}
