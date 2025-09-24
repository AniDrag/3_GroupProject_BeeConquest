using AniDrag.Utility;
using System;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using NUnit.Framework;

[System.Serializable]
public class BeeTypesForTheShopDontUseThisPlease
{
    public string BeeName;
    public long Cost;
    public Sprite beeSprite;
    public GameObject Bee;
    public GameObject beeCellImage;
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
    [SerializeField] public Transform beeSpawner;
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
    [SerializeField] public Transform cellHolder;
    private long _currentCellPrice;

    [Header("---------- Inventory uprade price settings ----------")]

    [SerializeField] private int maxInventoryUpgrades = 50;
    [SerializeField] private long inventoryUpgradeStartPrice = 1000;
    [SerializeField] private float inventoryPriceMulti = 1.98f;
    private long _currentInventoryPrice;
    private int _currentInventoryUpgrades;

    private int _emptyCells;
    private int _usedCells;
    private int _totalCells;
    private int _totalFoundCells;

    private List<GameObject> purchasedCells = new List<GameObject>();
    private List<GameObject> unPurchasedCells = new List<GameObject>();
    private List<GameObject> usedCells = new List<GameObject>();

    private const long PRICE_CAP = 100_000L;
    private void Awake()
    {
        upgradeInventory.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() => UpgradeInventory());
        upgradeCellCount.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() => BuyCell());
        SetUpBees();
        SetUpFood();
        UpdateCellCounter();
        SetUpCells();
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
        if (_player.currentHoneyAmount >= _currentInventoryPrice && maxInventoryUpgrades > _currentInventoryUpgrades)
        {
            _currentInventoryUpgrades++;
            _player.UpgradeMaxPollinStorage();
            _player.RemoveHoney(_currentInventoryPrice);
            _currentInventoryPrice = inventoryUpgradeStartPrice + (long)Mathf.Pow((inventoryUpgradeStartPrice * ((float)_player.pollinStorageLevel * inventoryPriceMulti)), _player.pollinStorageLevel / 20f);
            upgradeInventory.updatePrice(FormatNumber(_currentInventoryPrice));


            // Get the ShopItem that upgrades cells and update its price?
            //currentCells.text = "Cells: " + cellCounter.ToString();
            //buyCellButton.text = currentCellPrice.ToString();
        }
    }
    public void BuyCell()
    {
        if (_player == null)
        {
            Debug.Log("No player here yet");
            return;
        }

        if (_player.currentHoneyAmount >= _currentCellPrice && _totalCells < _totalFoundCells)
        {
            _totalCells++;
            _player.RemoveHoney(_currentCellPrice);

            // ----- CAP SETTINGS -----
            const long CELL_PRICE_CAP = 1_000_000_000_000_000_000L;

            // _currentCellPrice = cellStartPrice + (long)Mathf.Pow((cellStartPrice * ((float)_totalCells * cellMulti)), _totalCells / 20f);

            // Use double Math.Pow for more stable intermediate results, then clamp to cap:
            double baseVal = (double)cellStartPrice * ((double)_totalCells * (double)cellMulti);
            double exponent = (double)_totalCells / 20.0;
            double powResult = Math.Pow(baseVal, exponent); // same math shape, higher precision

            double newPriceDouble = (double)cellStartPrice + powResult;

            if (double.IsNaN(newPriceDouble) || double.IsInfinity(newPriceDouble) || newPriceDouble >= CELL_PRICE_CAP)
                _currentCellPrice = CELL_PRICE_CAP;
            else
                _currentCellPrice = (long)Math.Ceiling(newPriceDouble); // or (long)newPriceDouble if you prefer truncation

            _player.AddCell();
            UpdateCellCounter();

            upgradeCellCount.updatePrice(FormatNumber(_currentCellPrice));

            Debug.Log($"You have {_player.ownedCellsAmount - _player.allBees.Count} free cells");

            // pick a random unpurchased cell — use Count (exclusive max) so last item can be picked too
            if (unPurchasedCells.Count > 0)
            {
                GameObject newCell = unPurchasedCells[UnityEngine.Random.Range(0, unPurchasedCells.Count)];
                purchasedCells.Add(newCell);
                unPurchasedCells.Remove(newCell);
                newCell.SetActive(true);
            }
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


        GameObject cellForNewBee = purchasedCells[UnityEngine.Random.Range(0, purchasedCells.Count - 1)];
        usedCells.Add(cellForNewBee);
        purchasedCells.Remove(cellForNewBee);
        Instantiate(beesForSale[index].beeCellImage, cellForNewBee.transform.position, Quaternion.identity, cellForNewBee.transform);


        _player.BuyBee(beesForSale[index].Bee, cellForNewBee.transform);
        
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
            beeData.AsignData(beesForSale[i].BeeName, FormatNumber(beesForSale[i].Cost), beesForSale[i].beeSprite);

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
            materialData.AsignData(foodForSale[i].foodName, FormatNumber(foodForSale[i].Cost), foodForSale[i].foodSprite);

            int index = i; // capture index correctly
            materialData.button.onClick.AddListener(() => BuyFood(index));
        }
    }

    void SetUpCells()
    {
        var transforms = cellHolder.GetComponentsInChildren<Transform>(true);

        foreach (var t in transforms)
        {
            var go = t.gameObject;
            if (go == cellHolder) continue;
            if (go.name.StartsWith("Cell", StringComparison.OrdinalIgnoreCase))
                unPurchasedCells.Add(go);
        }

        if (unPurchasedCells.Count > 0)
            foreach (var cell in unPurchasedCells)
            {
                cell.SetActive(false);
            }
        _totalFoundCells = unPurchasedCells.Count();
        _totalCells = 0;
    }

    public static string FormatNumber(long amount)
    {
        if (amount < 1000) return amount.ToString();

        string[] suffixes = { "K", "M", "B", "T", "Q", "Qi", "Sx", "Sp", "Oc", "No", "Dc" }; // extend as needed
        double value = amount;
        int index = 0;

        while (value >= 1000 && index < suffixes.Length - 1)
        {
            value /= 1000.0;
            index++;
        }

        return value.ToString("0.##") + suffixes[index - 1];
    }
}
