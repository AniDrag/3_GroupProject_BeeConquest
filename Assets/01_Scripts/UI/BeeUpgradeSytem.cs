using AniDrag.Utility;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BeeUpgradeSytem : MonoBehaviour
{
    [Header("----- External Refrences -----")]
    [SerializeField] private PlayerCore player;
    [SerializeField] private GameObject InventoryItemPRF;
    [SerializeField] private Transform beePanel;
    [SerializeField] private Transform foodPanel;
    [SerializeField] private Transform spawnBeeInViewPort;
    [SerializeField] private TextMeshProUGUI statPointsLeftText;
    [SerializeField] private Slider beeXpBar;
    [SerializeField] private GameObject beeInViewPort;

    [Header("----- Stat Buttons -----")]
    [SerializeField] private Button UpgradeBee;
    [SerializeField] private Button Vitality;
    [SerializeField] private Button Strength;
    [SerializeField] private Button Dexterity;
    [SerializeField] private Button Agility;
    [SerializeField] private Button Luck;

    [Header("----- Visual Buttons -----")]
    [SerializeField] private Button zoomIN;
    [SerializeField] private Button zoomOUT;

    [Header("----- Input -----")]
    [SerializeField] private PlayerInput inputs;
    [SerializeField] private InputAction lookAction;   // Mouse Delta
    [SerializeField] private InputAction clickAction;  // Left Click

    private BeeFood selectedUpgradeConsumable;
    private BasicBee BeeBeingUpgraded;
    private int usedFoodIndex;
    private bool isDragging = false;
    private float rotationSpeed = 5f;
    private long xpToAdd;

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
        UpgradeBee.onClick.AddListener(() => FeedBee());
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
        if (beeInViewPort == null || !isDragging) return;

        Vector2 delta = lookAction.ReadValue<Vector2>();

        // Rotate around Y (horizontal drag)
        beeInViewPort.transform.Rotate(Vector3.up, -delta.x * rotationSpeed * Time.deltaTime, Space.World);

        // Rotate around X (vertical drag, clamped)
        beeInViewPort.transform.Rotate(Vector3.right, delta.y * rotationSpeed * Time.deltaTime, Space.World);
    }
    #endregion

    #region Bee Food Logic
    void FeedBee()
    {
        if(selectedUpgradeConsumable == null) return;
        BeeBeingUpgraded.AddXP(selectedUpgradeConsumable.heldXP);
        if(BeeBeingUpgraded._curentXP > BeeBeingUpgraded.XpToLevelUP)
        player.foodStorage[selectedUpgradeConsumable]--;
        foodPanel.GetChild(usedFoodIndex).GetComponent<InventoryItem>().UpdateText(player.foodStorage[selectedUpgradeConsumable].ToString());

        VusualizeStats();
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

       long tempVal= xpToAdd +  food.heldXP;
        if (tempVal <= beeXpBar.maxValue)
            xpToAdd = tempVal;
            beeXpBar.value = xpToAdd;
    }
    #endregion
    #region Bee Selection logic
    void SpawnAllBeesFromPlayerInventory()
    { 
        // Clear old buttons if needed
        foreach (Transform child in beePanel)
            Destroy(child.gameObject);

        for (int i = 0; i < player.savedBees.Count; i++)
        {
            int capturedIndex = i; // 👈 Fix closure issue

            GameObject newBee = Instantiate(InventoryItemPRF, beePanel);
            InventoryItem beeData = newBee.GetComponent<InventoryItem>();

            // Assign sprite (safety check in case sprite is missing)
            if (player.savedBees[capturedIndex].beeScritp != null)
                beeData.AsignData(player.savedBees[capturedIndex].beeScritp.BeeSprite);

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
        if (beeInViewPort != null)
            Destroy(beeInViewPort);

        // Instantiate the bee in the viewport (child[0] = actual model)
        GameObject beePrefab = player.savedBees[index].beeObject;
        if (beePrefab != null && beePrefab.transform.childCount > 0)
        {
            beeInViewPort = Instantiate(
                beePrefab.transform.GetChild(0).gameObject,
                spawnBeeInViewPort
            );
        }

        // Make sure allBees matches savedBees before indexing
        if (index < player.allBees.Count)
            BeeBeingUpgraded = player.allBees[index];
        else
            Debug.LogWarning($"allBees list out of sync with savedBees at index {index}");

        VusualizeStats();
    }
    void VusualizeStats()
    {
        Vitality.transform.parent.GetChild(1).GetComponent<TMP_Text>().text = $"Vitality: {BeeBeingUpgraded.Vitality}";
        Strength.transform.parent.GetChild(1).GetComponent<TMP_Text>().text = $"Strenght: {BeeBeingUpgraded.Strength}";
        Dexterity.transform.parent.GetChild(1).GetComponent<TMP_Text>().text = $"Dexterity: {BeeBeingUpgraded.Dexterity}";
        Agility.transform.parent.GetChild(1).GetComponent<TMP_Text>().text = $"Agility: {BeeBeingUpgraded.Agility}";
        Luck.transform.parent.GetChild(1).GetComponent<TMP_Text>().text = $"Luck: {BeeBeingUpgraded.Luck}";
    }
    void IncreseStat(StatType type)
    {
        BeeBeingUpgraded.StatIncrese(type);
    }
    #endregion
}
