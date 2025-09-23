using AniDrag.Utility;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Visuals : MonoBehaviour
{
    [Header("----- Refrences -----")]
    [SerializeField] private PlayerCore player;

    [Header("----- Input -----")]
    [SerializeField] private PlayerInput inputs;
    private InputAction menuAction;
    private InputAction inventoryAction;


    [Header("----- Playr Visuals -----")]
    [SerializeField] private bool enableVarsP;
    [SerializeField, ShowIf("enableVarsP")] public TMP_Text playerHealth;
    [SerializeField, ShowIf("enableVarsP")] public Slider playerHealthSlider;
    [Space]
    [SerializeField, ShowIf("enableVarsP")] public TMP_Text pollinCounterText;
    [SerializeField, ShowIf("enableVarsP")] public TMP_Text pollinPerSecText;
    [SerializeField, ShowIf("enableVarsP")] public Image backpakImage;
    [SerializeField, ShowIf("enableVarsP")] private float backpackFillLerpSpeed;
    [Space]
    [SerializeField, ShowIf("enableVarsP")] public TMP_Text honeyCounterText;
    [Space]
    [SerializeField, ShowIf("enableVarsP")] public Image crosHairImage;
    [Space]
    [SerializeField, ShowIf("enableVarsP")] public TMP_Text performanceText;

    [Header("----- Interaction Visuals -----")]
    [SerializeField] private bool enableVarsV;
    [SerializeField, ShowIf("enableVarsV")] public GameObject interactionBox;
    [SerializeField, ShowIf("enableVarsV")] public string interactionText = "Press → E ← to interact";
    [SerializeField, ShowIf("enableVarsV")] public TMP_Text interactedItemText;

    [Header("----- Menu & Options Visuals -----")]
    [SerializeField] private bool enableVarsMO;
    [SerializeField, ShowIf("enableVarsMO")] private GameObject menuPanel;   // Main menu panel
    [SerializeField, ShowIf("enableVarsMO")] private GameObject optionsPanel; // Options sub-menu
    [Space]
    [SerializeField, ShowIf("enableVarsMO")] private Button optionsBtn;
    [SerializeField, ShowIf("enableVarsMO")] private Button logOutBtn;
    [SerializeField, ShowIf("enableVarsMO")] private Button closeBtn;
    private bool menuOpen = false;
    [Header("----- Shop Visuals -----")]
    [SerializeField] bool enableVarsS = true;
    [SerializeField, Required, ShowIf("enableVarsS")] public GameObject shopPanel;
    [SerializeField] public GameObject inventoryPanel;
    private bool inventoryActive = false;

    private bool activeUI = false;
    #region ─────────────────────────────  Default Functions ───────────────────────────── 
    private void Awake()
    {
        if (menuPanel == null) Debug.LogError("❌ menuPanel is not assigned!", this);
        if (optionsPanel == null) Debug.LogError("❌ optionsPanel is not assigned!", this);
        if (optionsBtn == null) Debug.LogError("❌ optionsBtn is not assigned!", this);
        if (logOutBtn == null) Debug.LogError("❌ logOutBtn is not assigned!", this);
        if (closeBtn == null) Debug.LogError("❌ closeBtn is not assigned!", this);
        menuAction = inputs.actions["Menu"];
        inventoryAction = inputs.actions["Inventory"];

        if (menuAction == null)
            Debug.LogError("❌ Input Action 'Menu' not found in PlayerInput!");
        else
            menuAction.Enable();

        if (player == null)
        {
            Debug.LogError("❌ Input Action 'Menu' not found in PlayerInput!");
            player = transform.parent.GetComponent<PlayerCore>();
        }

        menuPanel.SetActive(false);
        optionsPanel.SetActive(false);
        shopPanel.SetActive(false);
        inventoryPanel.SetActive(false);
        // force PlayerInput to detect scheme
        if (string.IsNullOrEmpty(inputs.currentControlScheme))
            inputs.SwitchCurrentControlScheme("Keyboard&Mouse", Keyboard.current, Mouse.current);
        //Debug.Log("Scheme type: " + inputs.currentControlScheme + " || Is cursure visible: " + menuPanel.activeSelf);
        ApplyControlScheme(inputs.currentControlScheme);
    }
    private void OnEnable()
    {
        menuAction.performed += UI_ToggleMenu;
        inventoryAction.performed += UI_ToggleInventory;
        inputs.onControlsChanged += OnControlsChanged;

        optionsBtn.onClick.AddListener(UI_OpenOptions);
        logOutBtn.onClick.AddListener(UI_LogOut);
        closeBtn.onClick.AddListener(UI_CloseMenu);
    }
    private void OnDisable()
    {
        menuAction.performed -= UI_ToggleMenu;
        inventoryAction.performed -= UI_ToggleInventory;
        inputs.onControlsChanged -= OnControlsChanged;

        optionsBtn.onClick.RemoveListener(UI_OpenOptions);
        logOutBtn.onClick.RemoveListener(UI_LogOut);
        closeBtn.onClick.RemoveListener(UI_CloseMenu);
    }
    #endregion

    #region ─────────────────────────────  Pollin related Functions ───────────────────────────── 
    public void UI_UpdatePollin(long currentPollin, long maxPollin)
    {
        pollinCounterText.text = $"Pollen: {currentPollin} / {maxPollin}♣";
        float percent = (float)currentPollin / (float)maxPollin;
        StopAllCoroutines(); // avoid stacking multiple coroutines
        StartCoroutine(PollinLerp(percent));
        //Debug.Log("Updating visuals");
    }

    IEnumerator PollinLerp(float newPercent)
    {
        while (Mathf.Abs(backpakImage.fillAmount - newPercent) > 0.001f)
        {
            backpakImage.fillAmount = Mathf.Lerp(
                backpakImage.fillAmount,
                newPercent,
                Time.deltaTime * backpackFillLerpSpeed
            );
            yield return null; // wait one frame
        }
        backpakImage.fillAmount = newPercent; // snap to final value
    }
    #endregion
    #region ───────────────────────────── Menu & Options related Functions ───────────────────────────── 
    //───────────────────────────── Menu Toggle
    #region ───────────────────────────── Menu Toggle
    private void UI_ToggleMenu(InputAction.CallbackContext ctx)
    {
        Debug.Log("Menu action pressed!");
        if (menuOpen) UI_CloseMenu();
        else UI_OpenMenu();
    }
    public void UI_BTNToggleMenu()
    {
        Debug.Log("BTN Menu action pressed!");
        if (menuOpen) UI_CloseMenu();
        else UI_OpenMenu();
    }

    private void UI_OpenMenu()
    {
        menuOpen = true;
        activeUI = true;

        menuPanel.SetActive(true);
        optionsPanel.SetActive(false);

        ControllPlayerCamAndMove();
        ApplyControlScheme(inputs.currentControlScheme);

        Debug.Log("Enable menu. activeUI =" + activeUI);
    }

    private void UI_CloseMenu()
    {
        menuOpen = false;
        activeUI = inventoryPanel.activeSelf;

        menuPanel.SetActive(false);
        optionsPanel.SetActive(false);

        ControllPlayerCamAndMove();
        ApplyControlScheme(inputs.currentControlScheme);

        Debug.Log("Disable menu. activeUI =" +activeUI );
    }
    #endregion

    //───────────────────────────── Button Actions
    #region ───────────────────────────── Button Actions
    private void UI_OpenOptions() => optionsPanel.SetActive(true);

    private void UI_LogOut()
    {
        Debug.Log("Logging out...");
        Game_Manager.instance.LeaveServer(player.playerID); // replace with your actual function
        SceneManager.LoadSceneAsync(0);

    }

    #endregion
    //───────────────────────────── Player Control
    #region ───────────────────────────── Player Control
    public void ControllPlayerCamAndMove()
    {
        bool enableGameplay = !activeUI;

        // Example: disable movement + disable camera rotation
        // player.GetComponent<PlayerMovemant>().enabled = enableGameplay;
        Camera.main.GetComponent<PlayerCamera>().disableCamRotation = enableGameplay;
    }
    // ───────────────────────────── Input Switching
    private void OnControlsChanged(PlayerInput playerInput)
    {
        ApplyControlScheme(playerInput.currentControlScheme);
    }
    private void ApplyControlScheme(string scheme)
    {
        if (scheme == "Keyboard&Mouse")
        {
            if (activeUI) // Menu or Inventory is open
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
            }
            else // Gameplay
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }

            EventSystem.current.SetSelectedGameObject(null);
        }
        else if (scheme == "Gamepad")
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            if (activeUI)
                EventSystem.current.SetSelectedGameObject(optionsBtn.gameObject);
        }
    }
    #endregion

    #region  ────────────────────────  Interaction system
    // ───────────────────────────── Interaction system
    public void UI_ShowOrCloseInteractBox(bool state) =>
        interactionBox.gameObject.SetActive(state);

    public void UI_SetInteractText(string text) => interactedItemText.text = interactionText + text;
    #endregion
    #region  ─────────────────────────────  Inventory Toggle
    private void UI_ToggleInventory(InputAction.CallbackContext ctx)
    {
        if (menuOpen) return;

        Debug.Log("Inventory action pressed!");
        if (inventoryActive) UI_CloseInventory();
        else UI_OpenInventory();
    }
    public void UI_BTNToggleInventory()
    {
        Debug.Log("BTN Inventory action pressed!");
        if (inventoryActive) UI_CloseInventory();
        else UI_OpenInventory();
    }

    private void UI_OpenInventory()
    {
        inventoryActive = true;
        activeUI = true;

        inventoryPanel.SetActive(true);

        ControllPlayerCamAndMove();
        ApplyControlScheme(inputs.currentControlScheme);

        Debug.Log("Enable Inventory. activeUI =" + activeUI);
    }

    private void UI_CloseInventory()
    {
        inventoryActive = false;
        activeUI = menuPanel.activeSelf; // if menu is still open, keep UI active

        inventoryPanel.SetActive(false);

        ControllPlayerCamAndMove();
        ApplyControlScheme(inputs.currentControlScheme);
        Debug.Log("Disable Inventory. activeUI =" + activeUI);
    }
    #endregion
    #endregion
    #region ─────────────────────────────  Debug Button Fucnctions ───────────────────────────── 
    [Button("Test backpackLerp")]
    public void CallLerpFunction()
    {
        UI_UpdatePollin(10, 20);
    }
    [Button("Reset Backpack")]
    public void ResetBackpack()
    {
        UI_UpdatePollin(0, 20);
    }
    #endregion
}
