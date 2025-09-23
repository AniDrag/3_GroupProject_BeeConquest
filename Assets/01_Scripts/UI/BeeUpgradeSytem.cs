using AniDrag.Utility;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BeeUpgradeSytem : MonoBehaviour
{

    [Header("----- Inventory UI refrences -----")]
    [SerializeField] private GameObject InventoryItemPRF; 
    [SerializeField] private Transform beePanel;
    [SerializeField] private Transform foodPanel;

    [Header("----- Player Refrences -----")]
    [SerializeField] private PlayerCore player;
    [SerializeField] private List<PlayerBeeSaved> playerBees = new List<PlayerBeeSaved>();

    [Header("----- Bee UI Refrences -----")]
    [SerializeField] private Transform beeVisualInstanceTransform;
    [SerializeField] private GameObject beeVisualObject;

    [Header("----- Bee UI Stat Refrences -----")]
    [SerializeField] private TextMeshProUGUI statPointsLeftText;
    [SerializeField] private Slider beeXpBar;
    [SerializeField] private TextMeshProUGUI xpVaueText;
    [SerializeField] private Button zoomIN;
    [SerializeField] private Button zoomOUT;
    private long xpCach;

    [Header("----- Stat Buttons -----")]
    [SerializeField] private Button Vitality;
    [SerializeField] private Button Strength;
    [SerializeField] private Button Dexterity;
    [SerializeField] private Button Agility;
    [SerializeField] private Button Luck;

    [Header("----- Input -----")]
    [SerializeField] private PlayerInput inputs;
    [SerializeField] private InputAction lookAction;   // Mouse Delta
    [SerializeField] private InputAction clickAction;  // Left Click

    private BeeFood selectedFood;
    private BasicBee selectedBee;
    private int usedFoodIndex;
    private bool isDragging = false;
    private float rotationSpeed = 5f;

    private void Reset()
    {
        
    }
    void OnEnable()
    {
        clickAction.performed += OnClick;
        clickAction.canceled += OnRelease;
        SpawnAllFoodInInventory();
    }

    void OnDisable()
    {
        clickAction.performed -= OnClick;
        clickAction.canceled -= OnRelease;
    }
    void Start()
    {
        lookAction = inputs.actions["Look"];
        clickAction = inputs.actions["RightClick"];
        SpawnAllBeesFromPlayerInventory();
        SpawnAllFoodInInventory();
        Vitality.onClick.AddListener(() => IncreseStat(StatType.Vitality));
        Strength.onClick.AddListener(() => IncreseStat(StatType.Strength));
        Dexterity.onClick.AddListener(() => IncreseStat(StatType.Dexterity));
        Agility.onClick.AddListener(() => IncreseStat(StatType.Agility));
        Luck.onClick.AddListener(() => IncreseStat(StatType.Luck));
        SelectedBee(0);
    }

    // Update is called once per frame
    void Update()
    {
        ViewPortControlls();
    }
    #region Bee UI Visual Logic
    private void OnClick(InputAction.CallbackContext ctx)
    {
        // Optional: check if mouse is over bee viewport UI before allowing drag
        isDragging = true;
    }

    private void OnRelease(InputAction.CallbackContext ctx)
    {
        isDragging = false;
    }
    void ViewPortControlls()
    {
        if (beeVisualObject == null || !isDragging) return;

        Vector2 delta = lookAction.ReadValue<Vector2>();

        // Rotate around Y (horizontal drag)
        beeVisualObject.transform.Rotate(Vector3.up, -delta.x * rotationSpeed * Time.deltaTime, Space.World);

        // Rotate around X (vertical drag, clamped)
        beeVisualObject.transform.Rotate(Vector3.right, delta.y * rotationSpeed * Time.deltaTime, Space.World);
    }
    #endregion

    #region Bee Food Logic
    void FeedBee()
    {
        if(selectedFood == null) return;
        selectedBee.AddXP(selectedFood.heldXP);
        if(selectedBee._curentXP > selectedBee.XpToLevelUP)
        player.foodStorage[selectedFood]--;
        foodPanel.GetChild(usedFoodIndex).GetComponent<InventoryItem>().UpdateText(player.foodStorage[selectedFood].ToString());

        VisualizeStats();
    }
    void SpawnAllFoodInInventory()
    {
        // Clear old UI
        for (int i = foodPanel.childCount - 1; i >= 0; i--)
        {
            Destroy(foodPanel.GetChild(i).gameObject);
        }

        int index = 0;
        foreach (BeeFood food in player.foodStorage.Keys)
        {
            int capturedIndex = index; // ✅ fix closure bug

            GameObject newFood = Instantiate(InventoryItemPRF, foodPanel);
            InventoryItem foodData = newFood.GetComponent<InventoryItem>();

            foodData.AsignData(food.foodSprite);
            foodData.childIdex = capturedIndex;
            foodData.UpdateText(player.foodStorage[food].ToString());

            foodData.button.onClick.AddListener(() => SelectFoodAsUpgradeConsumable(food, capturedIndex));
            Debug.Log("saved index was: " + index);
            index++;
        }
    }
    void SelectFoodAsUpgradeConsumable(BeeFood food, int index)
    {
        if (player.foodStorage[food] > 0)
        {
            player.foodStorage[food]--;
            selectedBee.AddXP(food.heldXP);
            foodPanel.GetChild(index).GetComponent<InventoryItem>().UpdateText(player.foodStorage[food].ToString());
            long xp = selectedBee._curentXP + food.heldXP;
            VisualizeStats();
            UpdateXpBar();
        }
    }
    #endregion
    #region Bee Selection logic
    void SpawnAllBeesFromPlayerInventory()
    { 
        // Clear old buttons if needed
        foreach (Transform child in beePanel)
            Destroy(child.gameObject);
        playerBees.Clear();
        playerBees = player.savedBees;
        for (int i = 0; i < playerBees.Count; i++)
        {
            int capturedIndex = i; // 👈 Fix closure issue

            GameObject newBee = Instantiate(InventoryItemPRF, beePanel);
            InventoryItem beeData = newBee.GetComponent<InventoryItem>();

            // Assign sprite (safety check in case sprite is missing)
            if (playerBees[capturedIndex].beeScritp != null)
                beeData.AsignData(playerBees[capturedIndex].beeScritp.BeeSprite);

            // Correctly bind button click to this bee
            beeData.button.onClick.AddListener(() => SelectedBee(capturedIndex));
            Debug.Log("saved index was: " + capturedIndex);
        }
    }
    void SelectedBee(int index)
    {
        // Safety check
        if (index < 0 || index >= player.savedBees.Count)
        {
            Debug.LogWarning($"Invalid bee index: {index}");
            return;
        }

        // Clean up previously spawned bee in viewport
        if (beeVisualObject != null)
            Destroy(beeVisualObject);
        // Make sure allBees matches savedBees before indexing
        if (index < playerBees.Count)
            selectedBee = playerBees[index].beeScritp;
        else
            Debug.LogWarning($"allBees list out of sync with savedBees at index {index}");

        UpdateViewPort(index);
        UpdateXpBar();

        

        VisualizeStats();
    }
    void VisualizeStats()
    {
        statPointsLeftText.text = $"SP: {selectedBee.statPoints}";
        Vitality.transform.parent.GetChild(1).GetComponent<TMP_Text>().text = $"Vitality: {selectedBee.GetBeeVit}";
        Strength.transform.parent.GetChild(1).GetComponent<TMP_Text>().text = $"Strenght: {selectedBee.GetBeeStr}";
        Dexterity.transform.parent.GetChild(1).GetComponent<TMP_Text>().text = $"Dexterity: {selectedBee.GetBeeDex}";
        Agility.transform.parent.GetChild(1).GetComponent<TMP_Text>().text = $"Agility: {selectedBee.GetBeeAgi}";
        Luck.transform.parent.GetChild(1).GetComponent<TMP_Text>().text = $"Luck: {selectedBee.GetBeeLuc}";
    }
    void IncreseStat(StatType type)
    {
        if (selectedBee.statPoints > 0)
        {
            Debug.Log("Incresed stat: " + type.ToString());
            selectedBee.StatIncrese(type);
            selectedBee.statPoints--;
            VisualizeStats();
        }
    }

    void UpdateViewPort(int i)
    {
        GameObject beePrefab = player.savedBees[i].beeObject;
        if (beePrefab != null && beePrefab.transform.childCount > 0)
        {
            beeVisualObject = Instantiate(
                beePrefab.transform.GetChild(0).gameObject,
                beeVisualInstanceTransform
            );
        }
    }
    // Set up xp bar and update ui
    void UpdateXpBar()
    {
        beeXpBar.maxValue = selectedBee.XpToLevelUP;
        beeXpBar.value = selectedBee._curentXP;
        xpVaueText.text = $"XP {selectedBee._curentXP} / {selectedBee.XpToLevelUP} "; 
    }
    #endregion
}
